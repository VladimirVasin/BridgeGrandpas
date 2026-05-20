using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const int MaxNotebookObservations = 48;
    private const int CurrentObservationDay = 2;
    private const int ArchiveObservationDay = 1;
    private const float ObservationWritingSpeed = 15f;
    private const float ObservationWritingPause = 0.55f;
    private readonly List<NotebookObservation> notebookObservations = new List<NotebookObservation>();

    private sealed class NotebookObservation
    {
        public int Day;
        public float Time;
        public string Text;
        public bool Written;
        public bool HasClock;

        public NotebookObservation(int day, float time, string text, bool written, bool hasClock)
        {
            Day = day;
            Time = time;
            Text = text;
            Written = written;
            HasClock = hasClock;
        }
    }

    private void ResetNotebookObservations()
    {
        notebookObservations.Clear();
        ResetObservationLeads();
        ClearObservationCards();
        EnsureDayOneArchiveObservation();
    }

    private void AddNotebookObservation(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || NotebookObservationAlreadyWritten(text))
        {
            return;
        }

        notebookObservations.Add(new NotebookObservation(CurrentObservationDay, Time.time, text, false, true));
        TrimNotebookObservations();
    }

    private string LatestNotebookObservation()
    {
        for (int i = notebookObservations.Count - 1; i >= 0; i--)
        {
            if (notebookObservations[i].Day == CurrentObservationDay)
            {
                return notebookObservations[i].Text;
            }
        }

        return Time.time < alertUntil ? lastAlert : "День 2 пока пуст: карточки наблюдений ещё ждут вклейки.";
    }

    private bool NotebookObservationAlreadyWritten(string text)
    {
        for (int i = 0; i < notebookObservations.Count; i++)
        {
            if (notebookObservations[i].Text == text)
            {
                return true;
            }
        }

        return false;
    }

    private string BuildNotebookExpeditionIntro()
    {
        return "<b>Вылазки наверх</b>\n\n" +
            "Свободные дедушки: " + AvailableGrandpaCount() + "/" + grandpas.Count + "\n" +
            "В этой части блокнота отмечены случаи, когда кто-то уходит из-под моста в городскую сырость.\n\n" +
            "Картонщики лучше возвращаются с коробками, радиодеды слышат больше слухов, сторожа оставляют меньше следов.";
    }

    private void BuildNotebookObservationsSpread()
    {
        EnsureDayOneArchiveObservation();
        float writingDelay = 0f;
        int previousDay = Mathf.Max(ArchiveObservationDay, CurrentObservationDay - 1);

        notebookTitleText.text = "День " + previousDay;
        BuildNotebookObservationDay(notebookLeftPageContent, previousDay, false, ref writingDelay);
        AddNotebookText("<b>День " + CurrentObservationDay + "</b>", 18, FontStyle.Bold, 34f);
        BuildNotebookObservationDay(notebookPageContent, CurrentObservationDay, true, ref writingDelay);
    }

    private void BuildNotebookObservationDay(Transform parent, int day, bool animateNew, ref float writingDelay)
    {
        int count = NotebookObservationCountForDay(day);
        if (count == 0)
        {
            AddNotebookTextTo(parent, "Пока записей нет. Наблюдатель держит чистую страницу для новых карточек.",
                16, FontStyle.Italic, 56f);
            return;
        }

        for (int i = notebookObservations.Count - 1; i >= 0; i--)
        {
            NotebookObservation note = notebookObservations[i];
            if (note.Day != day)
            {
                continue;
            }

            string line = note.HasClock ? ObservationTime(note.Time) + " — " + note.Text : note.Text;
            Text noteText = AddNotebookTextTo(parent, line, 15, FontStyle.Normal, note.HasClock ? 52f : 320f);
            if (note.Written || !animateNew)
            {
                continue;
            }

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

    private int NotebookObservationCountForDay(int day)
    {
        int count = 0;
        for (int i = 0; i < notebookObservations.Count; i++)
        {
            if (notebookObservations[i].Day == day)
            {
                count++;
            }
        }

        return count;
    }

    private void EnsureDayOneArchiveObservation()
    {
        string text = DayOneArchiveObservationText();
        if (NotebookObservationAlreadyWritten(text))
        {
            return;
        }

        notebookObservations.Insert(0, new NotebookObservation(ArchiveObservationDay, 0f, text, true, false));
    }

    private string DayOneArchiveObservationText()
    {
        return "Дождливым и промозглым ноябрьским вечером я возвращался с работы мимо старого моста.\n\n" +
            "Ветер донёс до меня звук затухающего костра и глухое старческое ворчание. Что-то насторожило меня; я остановился и начал всматриваться.\n\n" +
            "У бочки сидел дедушка с кружкой чая. Его пальто странно шевелилось в свете огня, который он развёл прямо в ржавой бочке, чтобы согреться.\n\n" +
            "Он отпил, наклонился — и... не знаю, как это описать, но разделился надвое.\n\n" +
            "Через мгновение у бочки сидели уже двое одинаковых дедушек. Первый молча налил второму чаю.\n\n" +
            "Вне всякого сомнения, они размножаются почкованием.\n\n" +
            "Отныне, что бы ни случилось, я должен наблюдать, чтобы раскрыть их тайну.";
    }

    private void TrimNotebookObservations()
    {
        while (notebookObservations.Count > MaxNotebookObservations)
        {
            int removeIndex = -1;
            for (int i = 0; i < notebookObservations.Count; i++)
            {
                if (notebookObservations[i].Day != ArchiveObservationDay)
                {
                    removeIndex = i;
                    break;
                }
            }

            if (removeIndex < 0)
            {
                return;
            }

            notebookObservations.RemoveAt(removeIndex);
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
