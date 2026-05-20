using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const string PlansOldMenObservationLabel = "Планы стариков";
    private const string PlansOldMenObservationText = "Два деда похоже настроены серьезно. Кажется они тянут жребий?";
    private const string LegacyPlansOldMenObservationText = "Два деда это уже начало чего то нового. Что же они замышляют?";
    private const string PlansOldMenCollectorSuffix = " отправился собирать всякий хлам";

    private bool plansOldMenFollowupOpen;
    private bool plansOldMenFollowupResolved;
    private int plansOldMenCollectorGrandpaId = -1;

    private void ResetPlansOldMenJournalAction()
    {
        plansOldMenFollowupOpen = false;
        plansOldMenFollowupResolved = false;
        plansOldMenCollectorGrandpaId = -1;
    }

    private bool CanShowPlansOldMenFollowup(NotebookObservation note, bool animateNew)
    {
        if (note == null || note.Day != CurrentObservationDay || !note.HasClock)
        {
            return false;
        }

        if (!IsPlansOldMenObservationText(note.Text) || PlansOldMenFollowupAlreadyResolved())
        {
            return false;
        }

        return note.Written || !animateNew;
    }

    private void AddPlansOldMenFollowupControls(Transform parent)
    {
        RectTransform block = CreatePanel("Plans Old Men Followup", parent, new Color(0.44f, 0.25f, 0.11f, 0.16f));
        Image image = block.GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = false;
        }

        LayoutElement layout = block.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = plansOldMenFollowupOpen ? 176f : 64f;
        layout.preferredHeight = layout.minHeight;
        layout.flexibleWidth = 1f;

        VerticalLayoutGroup vertical = block.gameObject.AddComponent<VerticalLayoutGroup>();
        vertical.padding = new RectOffset(8, 8, 6, 6);
        vertical.spacing = 6f;
        vertical.childControlWidth = true;
        vertical.childControlHeight = true;
        vertical.childForceExpandWidth = true;
        vertical.childForceExpandHeight = false;

        CreateNotebookButton(plansOldMenFollowupOpen ? "Что было дальше?  ▲" : "Что было дальше?  ▼",
            block, TogglePlansOldMenFollowup);

        if (!plansOldMenFollowupOpen)
        {
            return;
        }

        AddPlansOldMenChoiceButton(block, PlansOldMenGrandpaAtIndex(0));
        AddPlansOldMenChoiceButton(block, PlansOldMenGrandpaAtIndex(1));
    }

    private void AddPlansOldMenChoiceButton(Transform parent, Grandpa grandpa)
    {
        if (grandpa == null)
        {
            return;
        }

        CreateNotebookButton(grandpa.Name + PlansOldMenCollectorSuffix, parent,
            delegate { ChoosePlansOldMenCollector(grandpa); });
    }

    private void TogglePlansOldMenFollowup()
    {
        plansOldMenFollowupOpen = !plansOldMenFollowupOpen;
        if (notebookModeEnabled && currentNotebookPage == NotebookPage.Observations)
        {
            RefreshNotebookUi();
            return;
        }

        MarkNotebookDirty();
    }

    private void ChoosePlansOldMenCollector(Grandpa grandpa)
    {
        if (grandpa == null)
        {
            return;
        }

        if (grandpa.IsOnExpedition)
        {
            Notify(grandpa.Name + " сейчас не под мостом. Жребий ждёт живого присутствия.");
            return;
        }

        string observation = grandpa.Name + PlansOldMenCollectorSuffix;
        plansOldMenFollowupResolved = true;
        plansOldMenCollectorGrandpaId = grandpa.Id;
        plansOldMenFollowupOpen = false;
        WriteDebugLog("JOURNAL_CHOICE", "Plans old men collector selected: " + DebugGrandpaSnapshot(grandpa));

        AddNotebookObservation(observation);
        SetGrandpaWorkMode(grandpa, GrandpaWorkMode.JunkCollector);
        ShowThought(grandpa, "жребий есть жребий", 2.2f);

        if (notebookModeEnabled && currentNotebookPage == NotebookPage.Observations)
        {
            RefreshNotebookUi();
        }
    }

    private Grandpa PlansOldMenGrandpaAtIndex(int index)
    {
        int seen = 0;
        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa grandpa = grandpas[i];
            if (grandpa == null)
            {
                continue;
            }

            if (seen == index)
            {
                return grandpa;
            }

            seen++;
        }

        return null;
    }

    private bool IsPlansOldMenObservationText(string text)
    {
        return text == PlansOldMenObservationText || text == LegacyPlansOldMenObservationText;
    }

    private bool IsPlansOldMenChoiceText(string text)
    {
        return !string.IsNullOrEmpty(text) && text.EndsWith(PlansOldMenCollectorSuffix);
    }

    private bool PlansOldMenFollowupAlreadyResolved()
    {
        if (plansOldMenFollowupResolved)
        {
            return true;
        }

        for (int i = 0; i < notebookObservations.Count; i++)
        {
            NotebookObservation note = notebookObservations[i];
            if (note.Day == CurrentObservationDay && note.HasClock && IsPlansOldMenChoiceText(note.Text))
            {
                return true;
            }
        }

        return false;
    }
}
