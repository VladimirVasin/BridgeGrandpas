using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const int MaxNotebookObservations = 48;
    private const float ObservationWritingSpeed = 15f;
    private const float ObservationWritingPause = 0.55f;
    private readonly List<NotebookObservation> notebookObservations = new List<NotebookObservation>();

    private sealed class NotebookObservation
    {
        public float Time;
        public string Text;
        public bool Written;

        public NotebookObservation(float time, string text)
        {
            Time = time;
            Text = text;
            Written = false;
        }
    }

    private void ResetNotebookObservations()
    {
        notebookObservations.Clear();
        ResetObservationLeads();
        QueueObservationLead("сухое пятно", "Под мостом обнаружено сухое пятно. Оно уже выглядит подозрительно пригодным для жизни.",
            null, DefaultObservationPosition(), 0f);
    }

    private void AddNotebookObservation(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        notebookObservations.Add(new NotebookObservation(Time.time, text));
        while (notebookObservations.Count > MaxNotebookObservations)
        {
            notebookObservations.RemoveAt(0);
        }
    }

    private string LatestNotebookObservation()
    {
        if (notebookObservations.Count == 0)
        {
            return Time.time < alertUntil ? lastAlert : "Под мостом пока только мокрый воздух и терпение.";
        }

        return notebookObservations[notebookObservations.Count - 1].Text;
    }

    private string BuildNotebookObservationsIntro()
    {
        return "Это не меню приказов. Это блокнот человека, который делает вид, что просто записывает факты.\n\n" +
            "Новые записи появляются только после VHS-наблюдения: включи камеру, поймай цель в центре кадра и удерживай ЛКМ или Space.";
    }

    private string BuildNotebookExpeditionIntro()
    {
        return "<b>Вылазки наверх</b>\n\n" +
            "Свободные дедушки: " + AvailableGrandpaCount() + "/" + grandpas.Count + "\n" +
            "В этой части блокнота отмечены случаи, когда кто-то уходит из-под моста в городскую сырость.\n\n" +
            "Картонщики лучше возвращаются с коробками, радиодеды слышат больше слухов, сторожа оставляют меньше следов.";
    }

    private void BuildNotebookObservationsPage()
    {
        if (notebookObservations.Count == 0)
        {
            AddNotebookText("Пока записей нет. Наблюдатель ещё выбирает, чем пахнет мокрый асфальт.", 16, FontStyle.Italic, 60f);
            return;
        }

        float writingDelay = 0f;
        for (int i = notebookObservations.Count - 1; i >= 0; i--)
        {
            NotebookObservation note = notebookObservations[i];
            string line = ObservationTime(note.Time) + " — " + note.Text;
            Text noteText = AddNotebookText(line, 15, FontStyle.Normal, 46f);
            if (!note.Written)
            {
                BridgeGrandpasNotebookWritingText writing = noteText.gameObject.AddComponent<BridgeGrandpasNotebookWritingText>();
                int observationIndex = i;
                string observedText = note.Text;
                writing.Play(noteText, line, writingDelay, ObservationWritingSpeed, delegate
                {
                    MarkNotebookObservationWritten(observationIndex, observedText);
                });
                writingDelay += Mathf.Max(1.4f, line.Length / ObservationWritingSpeed) + ObservationWritingPause;
            }
        }
    }

    private void MarkNotebookObservationWritten(int index, string text)
    {
        if (index < 0 || index >= notebookObservations.Count)
        {
            return;
        }

        if (notebookObservations[index].Text != text)
        {
            return;
        }

        notebookObservations[index].Written = true;
    }

    private string ObservationTime(float time)
    {
        int seconds = Mathf.Max(0, Mathf.FloorToInt(time));
        int minutes = seconds / 60;
        seconds %= 60;
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private string NotebookBuildPhrase(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.Samovar:
                return "Был построен самовар";
            case BuildingType.Bedroom:
                return "Была устроена картонная спальня";
            case BuildingType.GrumbleBench:
                return "Была поставлена скамейка ворчания";
            case BuildingType.CarpetCurtain:
                return "Был натянут занавес из ковров";
            case BuildingType.RadioMayak:
                return "Было подключено радио \"Маяк\"";
            default:
                return "Была разведена бочка с огнём";
        }
    }

    private string NotebookUpgradePhrase(Building building)
    {
        return "В записи отмечено: " + NotebookObjectName(building.Type) + " стал крепче, ур. " +
            building.Level + " -> " + (building.Level + 1);
    }

    private string NotebookObjectName(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.Samovar:
                return "самовар";
            case BuildingType.Bedroom:
                return "картонная спальня";
            case BuildingType.GrumbleBench:
                return "скамейка ворчания";
            case BuildingType.CarpetCurtain:
                return "ковровый занавес";
            case BuildingType.RadioMayak:
                return "радио \"Маяк\"";
            default:
                return "бочка с огнём";
        }
    }

    private string NotebookGrandpaObservation(Grandpa grandpa)
    {
        return grandpa.Name + " замечен под мостом | " + RoleName(grandpa.Role);
    }

    private string NotebookRoleObservation(GrandpaRole role)
    {
        switch (role)
        {
            case GrandpaRole.SamovarKeeper:
                return "Чаще всего замечен у самовара; после него чай появляется убедительнее.";
            case GrandpaRole.Cardboarder:
                return "Тянется к коробкам и краям сцены, будто картон сам его зовёт.";
            case GrandpaRole.Mutterer:
                return "Сидит с видом человека, который уже всё понял и теперь бормочет выводы.";
            case GrandpaRole.Guard:
                return "Держится ближе к границе и делает город чуть менее уверенным.";
            case GrandpaRole.Philosopher:
                return "Записывать рядом осторожно: мысли странные, но иногда полезные.";
            case GrandpaRole.RadioReceiver:
                return "Похож на дедушку, который слышит город раньше остальных.";
            default:
                return "Обычный дед, но обычность под мостом быстро становится подозрительной.";
        }
    }

    private string NotebookExpeditionPlan(Grandpa grandpa, ExpeditionType type)
    {
        switch (type)
        {
            case ExpeditionType.CoinAdvice:
                return grandpa.Name + " будет давать советы прохожим";
            case ExpeditionType.TeaSalvage:
                return grandpa.Name + " полезет искать чайные остатки";
            case ExpeditionType.CityRumors:
                return grandpa.Name + " будет слушать город";
            default:
                return grandpa.Name + " полезет за картоном";
        }
    }

    private string PlainNotebookText(string richText)
    {
        if (string.IsNullOrEmpty(richText))
        {
            return "";
        }

        bool insideTag = false;
        List<char> chars = new List<char>(richText.Length);
        for (int i = 0; i < richText.Length; i++)
        {
            char c = richText[i];
            if (c == '<')
            {
                insideTag = true;
                continue;
            }

            if (c == '>')
            {
                insideTag = false;
                continue;
            }

            if (!insideTag)
            {
                chars.Add(c);
            }
        }

        return new string(chars.ToArray()).Replace("|", ",");
    }
}
