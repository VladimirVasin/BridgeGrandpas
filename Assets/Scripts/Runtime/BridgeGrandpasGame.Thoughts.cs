using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private static readonly string[] GeneralDepressiveThoughts =
    {
        "Мост помнит больше, чем я",
        "Сырость сегодня внутри",
        "Город наверху живёт громко",
        "Нас опять не внесли в план",
        "Картон теплее некоторых людей",
        "Чай остыл раньше надежды",
        "Тишина тоже шумит",
        "Я был моложе при другом асфальте",
        "Под мостом хотя бы честно",
        "Дождь знает моё расписание",
        "Жизнь пахнет мокрой газетой",
        "Никто не спросил, как мост",
        "Сверху идут, снизу терпим",
        "Сегодня даже пар грустит",
        "Мир большой, а коробка мала",
        "Мечты отсырели",
        "Скамейка понимает без слов",
        "Ветер опять считает нас лишними",
        "Если молчать, слышно старость",
        "Понедельник поселился рядом"
    };

    private static readonly string[] GeneralNeutralThoughts =
    {
        "Бочка сегодня держится бодро",
        "Картон подсох, можно жить",
        "Сверху шумят, снизу чай",
        "Мост скрипит, но привычно",
        "Надо бы поправить коврик",
        "Газета стала мягче после дождя",
        "Пока тепло, философия терпит",
        "Кружка нашла своё место",
        "Ворчание идёт ровно",
        "Скамейка почти не спорит"
    };

    private static readonly string[] GeneralCozyThoughts =
    {
        "Тут уже почти хорошо",
        "Огонь сегодня добрый",
        "Под мостом стало мягче",
        "Чай пахнет как выходной",
        "Коврик держит душу на месте",
        "Можно ворчать без отчаяния",
        "Мы тут неплохо устроились",
        "Даже дождь звучит уютно",
        "Старость любит тёплый угол",
        "Сегодня коробка похожа на дом",
        "Свет от бочки всё прощает",
        "Я бы тут ещё посидел",
        "Уют — это когда не хочется уходить",
        "Под мостом наконец-то по-человечески странно",
        "Носки сохнут, жизнь продолжается"
    };

    private static readonly string[] BuddingThoughts =
    {
        "Я почти готов почковаться",
        "Во мне копится ещё один я",
        "Скоро станет на одного грустнее",
        "Почка просит чаю",
        "Размножение тоже устало"
    };

    private static readonly string[] TeaLowThoughts =
    {
        "Без чая мысли ржавеют",
        "Чай кончился, смысл тоже",
        "Пустая кружка смотрит в душу",
        "Я слышу сухой самовар",
        "Нечем запивать реальность"
    };

    private static readonly string[] ColdThoughts =
    {
        "Холод считает мои пуговицы",
        "Шарф держится из последних сил",
        "Тепло ушло по делам",
        "Кости спорят с погодой",
        "Огонь слишком далеко от сердца"
    };

    private static readonly string[] SuspicionThoughts =
    {
        "Сверху слишком внимательно",
        "Комиссия пахнет бумагой",
        "Притворяюсь инфраструктурой",
        "Нас скоро назовут нарушением",
        "Тихо, город смотрит вниз"
    };

    private static readonly string[] SamovarThoughts =
    {
        "Самовар тоже одинок",
        "Пар поднимается, а мы нет",
        "Чай кипит без радости",
        "Самовар - это сердце",
        "Кипяток лучше разговоров"
    };

    private static readonly string[] CardboardThoughts =
    {
        "Картон - основа цивилизации",
        "Коробка честнее квартиры",
        "У картона нет иллюзий",
        "Этот лист видел магазин",
        "Строю стену от будущего"
    };

    private static readonly string[] MuttererThoughts =
    {
        "Пора ворчать",
        "Ворчание держит форму мира",
        "Я бормочу, значит существую",
        "Молчание слишком дорого",
        "Недовольство согревает хуже чая"
    };

    private static readonly string[] GuardThoughts =
    {
        "Сверху шумят",
        "Город делает вид, что не видит",
        "Я охраняю нашу незаметность",
        "Тень сегодня ненадёжная",
        "Ковёр спасёт не всех"
    };

    private static readonly string[] PhilosopherThoughts =
    {
        "Тёплая мысль греет дважды",
        "Бытие отсырело",
        "Под мостом истина ниже",
        "Смысл требует картона",
        "Мудрость пахнет дымом"
    };

    private static readonly string[] RadioThoughts =
    {
        "Ш-ш-ш... комиссия...",
        "Эфир полон плохих новостей",
        "Город шепчет не нам",
        "Радио ловит чужую тревогу",
        "Волны принесли усталость"
    };

    private string RandomThought(Grandpa grandpa)
    {
        float warmth = Mathf.InverseLerp(4f, 52f, cozyScore);
        if (suspicion > 82f && Roll(Mathf.Lerp(0.65f, 0.35f, warmth)))
        {
            return PickThought(SuspicionThoughts);
        }

        if (stock.Tea < 4f && Roll(Mathf.Lerp(0.55f, 0.30f, warmth)))
        {
            return PickThought(TeaLowThoughts);
        }

        if (stock.Heat < 5f && Roll(Mathf.Lerp(0.55f, 0.30f, warmth)))
        {
            return PickThought(ColdThoughts);
        }

        if (grandpa.Budding > 86f)
        {
            return PickThought(BuddingThoughts);
        }

        if (Roll(warmth * 0.62f))
        {
            return PickThought(GeneralCozyThoughts);
        }

        if (Roll(warmth * 0.42f))
        {
            return PickThought(GeneralNeutralThoughts);
        }

        if (Roll(0.45f))
        {
            return PickRoleThought(grandpa.Role);
        }

        return warmth > 0.45f ? PickThought(GeneralNeutralThoughts) : PickThought(GeneralDepressiveThoughts);
    }

    private string PickRoleThought(GrandpaRole role)
    {
        switch (role)
        {
            case GrandpaRole.SamovarKeeper:
                return PickThought(SamovarThoughts);
            case GrandpaRole.Cardboarder:
                return PickThought(CardboardThoughts);
            case GrandpaRole.Mutterer:
                return PickThought(MuttererThoughts);
            case GrandpaRole.Guard:
                return PickThought(GuardThoughts);
            case GrandpaRole.Philosopher:
                return PickThought(PhilosopherThoughts);
            case GrandpaRole.RadioReceiver:
                return PickThought(RadioThoughts);
            default:
                return PickThought(GeneralDepressiveThoughts);
        }
    }

    private string PickThought(string[] thoughts)
    {
        return thoughts[random.Next(thoughts.Length)];
    }
}
