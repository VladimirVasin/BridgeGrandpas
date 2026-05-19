using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private void StartExpeditionDiceRoll(Grandpa grandpa, float reward, float risk, string result)
    {
        if (expeditionDiceRolling || expeditionDiceResultVisible || grandpa == null)
        {
            return;
        }

        expeditionDiceRolling = true;
        expeditionDiceResultVisible = false;
        expeditionDiceStart = Time.time;
        expeditionDiceUntil = Time.time + 1.18f;
        expeditionDiceCloseAt = expeditionDiceUntil + 1.05f;
        expeditionDiceResult = UnityEngine.Random.Range(1, 7);
        expeditionDiceGrandpa = grandpa;
        expeditionDiceRewardMultiplier = reward;
        expeditionDiceRiskMultiplier = risk;
        expeditionDiceResultText = result;
        grandpa.ExpeditionUntil = Mathf.Max(grandpa.ExpeditionUntil, expeditionDiceCloseAt + 0.35f);
        SetExpeditionChoiceButtons(false);

        if (expeditionDicePanel != null)
        {
            expeditionDicePanel.gameObject.SetActive(true);
        }

        if (expeditionDiceCaptionText != null)
        {
            expeditionDiceCaptionText.text = "Кубик стучит по мокрому асфальту...";
            expeditionDiceCaptionText.gameObject.SetActive(true);
        }

        MarkNotebookDirty();
    }

    private void UpdateExpeditionDice(float deltaTime)
    {
        if ((!expeditionDiceRolling && !expeditionDiceResultVisible) || expeditionDicePanel == null)
        {
            return;
        }

        if (expeditionDiceResultVisible)
        {
            UpdateExpeditionDiceResultPose();
            if (Time.time >= expeditionDiceCloseAt)
            {
                ApplyExpeditionDiceResult();
            }

            return;
        }

        if (Time.time >= expeditionDiceUntil)
        {
            ShowExpeditionDiceResult();
            return;
        }

        float t = Mathf.InverseLerp(expeditionDiceStart, expeditionDiceUntil, Time.time);
        float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 5.5f)) * (1f - t);
        expeditionDicePanel.anchoredPosition = new Vector2(Mathf.Sin(Time.time * 18f) * 10f, -22f + bounce * 34f);
        expeditionDicePanel.localRotation = Quaternion.Euler(0f, 0f, Time.time * 820f);
        float scale = 1f + bounce * 0.45f + Mathf.Sin(Time.time * 31f) * 0.06f;
        expeditionDicePanel.localScale = Vector3.one * scale;

        if (expeditionDiceText != null)
        {
            expeditionDiceText.text = UnityEngine.Random.Range(1, 7).ToString();
        }

        if (expeditionDiceCaptionText != null)
        {
            Color color = expeditionDiceCaptionText.color;
            color.a = 0.72f + bounce * 0.28f;
            expeditionDiceCaptionText.color = color;
        }
    }

    private void ShowExpeditionDiceResult()
    {
        expeditionDiceRolling = false;
        expeditionDiceResultVisible = true;
        expeditionDiceCloseAt = Time.time + 1.05f;
        if (expeditionDiceText != null)
        {
            expeditionDiceText.text = expeditionDiceResult.ToString();
        }

        if (expeditionDiceCaptionText != null)
        {
            expeditionDiceCaptionText.text = "Выпало " + expeditionDiceResult + ". Дедушка принимает судьбу.";
            Color color = expeditionDiceCaptionText.color;
            color.a = 1f;
            expeditionDiceCaptionText.color = color;
        }

        UpdateExpeditionDiceResultPose();
    }

    private void UpdateExpeditionDiceResultPose()
    {
        float pulse = Mathf.Sin(Time.time * 9f) * 0.035f;
        expeditionDicePanel.anchoredPosition = new Vector2(0f, -22f);
        expeditionDicePanel.localRotation = Quaternion.Lerp(expeditionDicePanel.localRotation, Quaternion.identity, 0.24f);
        expeditionDicePanel.localScale = Vector3.one * (1.14f + pulse);
    }

    private void ApplyExpeditionDiceResult()
    {
        Grandpa grandpa = expeditionDiceGrandpa;
        float luckReward = 0.85f + expeditionDiceResult * 0.06f;
        float luckRisk = 1.25f - expeditionDiceResult * 0.07f;

        if (grandpa != null)
        {
            grandpa.ExpeditionRewardMultiplier *= expeditionDiceRewardMultiplier * luckReward;
            grandpa.ExpeditionRiskMultiplier *= expeditionDiceRiskMultiplier * luckRisk;
            grandpa.ExpeditionNarrativeResolved = true;
            Notify(grandpa.Name + " " + expeditionDiceResultText + ". Кубик: " + expeditionDiceResult + ".");
            QueueObservationLead("кубик вылазки", "Вылазка " + grandpa.Name + ": кубик показал " +
                expeditionDiceResult + ". Записано: " + expeditionDiceResultText + ".",
                null, grandpa.ExpeditionExitPosition, 0.18f);
        }

        if (expeditionDiceText != null)
        {
            expeditionDiceText.text = expeditionDiceResult.ToString();
        }

        expeditionDiceRolling = false;
        expeditionDiceResultVisible = false;
        SetExpeditionChoiceButtons(true);
        ResetExpeditionDice();
        if (expeditionModal != null)
        {
            expeditionModal.gameObject.SetActive(false);
        }

        MarkNotebookDirty();
        RefreshAllUi();
    }

    private void ResetExpeditionDice()
    {
        expeditionDiceRolling = false;
        expeditionDiceResultVisible = false;
        expeditionDiceGrandpa = null;
        expeditionDiceRewardMultiplier = 1f;
        expeditionDiceRiskMultiplier = 1f;
        expeditionDiceResultText = "";
        if (expeditionDicePanel != null)
        {
            expeditionDicePanel.anchoredPosition = new Vector2(0f, -22f);
            expeditionDicePanel.localRotation = Quaternion.identity;
            expeditionDicePanel.localScale = Vector3.one;
            expeditionDicePanel.gameObject.SetActive(false);
        }

        if (expeditionDiceCaptionText != null)
        {
            Color color = expeditionDiceCaptionText.color;
            color.a = 1f;
            expeditionDiceCaptionText.color = color;
            expeditionDiceCaptionText.gameObject.SetActive(false);
        }
    }

    private void SetExpeditionChoiceButtons(bool interactable)
    {
        if (expeditionChoicesRoot == null)
        {
            return;
        }

        for (int i = 0; i < expeditionChoicesRoot.childCount; i++)
        {
            Button button = expeditionChoicesRoot.GetChild(i).GetComponent<Button>();
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }
}
