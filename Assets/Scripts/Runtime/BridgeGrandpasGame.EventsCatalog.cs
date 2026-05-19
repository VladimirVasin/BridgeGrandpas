using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;
public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private void SetupEvents()
    {
        events.Add(new BridgeEvent(
            "Сверху прошли школьники",
            "Дети заметили движение под мостом и спорят, куча это картона или дедовская астрономия.",
            new EventChoice("Помахать им", "<color=#9cff93>+12 ворчания</color> | <color=#ff8f7a>+14 подозрения</color>", delegate(BridgeGrandpasGame game)
            {
                game.stock.Grumble += 12f;
                game.suspicion += 14f;
                game.Notify("+12 ворчания, но город стал подозрительнее.");
            }),
            new EventChoice("Притвориться кучей картона", "<color=#9cff93>-12 подозрения</color>", delegate(BridgeGrandpasGame game)
            {
                game.suspicion -= 12f;
                game.Notify("Дедушки убедительно стали прямоугольными. Подозрение снизилось.");
            }),
            new EventChoice("Коллективно заворчать", "<color=#9cff93>+18 ворчания</color> | <color=#ffcf7a>35% риск +8 подозрения</color>", delegate(BridgeGrandpasGame game)
            {
                game.stock.Grumble += 18f;
                if (game.Roll(0.35f))
                {
                    game.suspicion += 8f;
                    game.Notify("+18 ворчания. Кто-то наверху записал звук на телефон.");
                }
                else
                {
                    game.Notify("+18 ворчания. Школьники решили, что это старый кондиционер.");
                }
            })));

        events.Add(new BridgeEvent(
            "На мосту ремонт",
            "Рабочие оставили доски, мешки и мнение о жизни.",
            new EventChoice("Стащить материалы", "<color=#9cff93>+22 картона</color> | <color=#ff8f7a>+10 подозрения</color>", delegate(BridgeGrandpasGame game)
            {
                game.stock.Cardboard += 22f;
                game.suspicion += 10f;
                game.Notify("+22 картона. Очень строительный запах подозрения.");
            }),
            new EventChoice("Дать рабочим совет", "<color=#9cff93>+3 монетки</color> | <color=#9cff93>+5 ворчания</color>", delegate(BridgeGrandpasGame game)
            {
                game.stock.Coins += 3f;
                game.stock.Grumble += 5f;
                game.Notify("+3 монетки. Советы оказались платными.");
            }),
            new EventChoice("Спрятаться глубже", "<color=#9cff93>-16 подозрения</color> | <color=#ffcf7a>случайный объект замрёт на 20с</color>", delegate(BridgeGrandpasGame game)
            {
                game.suspicion -= 16f;
                game.BlockRandomBuilding(20f);
                game.Notify("Подозрение снижено, но одна зона временно замерла.");
            })));

        events.Add(new BridgeEvent(
            "Один дед вспомнил молодость",
            "История началась с фразы \"а вот раньше мосты были честнее\".",
            new EventChoice("Записать байку", "<color=#9cff93>+20 ворчания</color>", delegate(BridgeGrandpasGame game)
            {
                game.stock.Grumble += 20f;
                game.Notify("+20 ворчания. Байка внесена в устный архив.");
            }),
            new EventChoice("Дать ему чаю", "<color=#ffcf7a>-4 чая</color> | <color=#9cff93>всем дедушкам +12% почкования</color>", delegate(BridgeGrandpasGame game)
            {
                game.stock.Tea = Mathf.Max(0f, game.stock.Tea - 4f);
                game.AddBuddingToAll(12f);
                game.Notify("Все дедушки стали на 12% ближе к почкованию.");
            }),
            new EventChoice("Посадить к философу", "<color=#9cff93>идея философа</color> | <color=#ffcf7a>если философа нет: 35% шанс</color>", delegate(BridgeGrandpasGame game)
            {
                if (game.CountRole(GrandpaRole.Philosopher) > 0 || game.Roll(0.35f))
                {
                    game.UnlockPhilosophicalIdea();
                }
                else
                {
                    game.stock.Grumble += 8f;
                    game.Notify("Философии пока нет, но ворчание стало глубже.");
                }
            })));

        events.Add(new BridgeEvent(
            "Инспектор смотрит вниз",
            "С моста свисает лицо человека, который явно умеет составлять акт.",
            new EventChoice("Задёрнуть ковры", "<color=#9cff93>-18 подозрения</color>", delegate(BridgeGrandpasGame game)
            {
                game.suspicion -= 18f;
                game.Notify("Ковры приняли удар бюрократии на себя.");
            }),
            new EventChoice("Погасить бочку", "<color=#9cff93>-24 подозрения</color> | <color=#ff8f7a>-18 тепла, бочка заблокирована 24с</color>", delegate(BridgeGrandpasGame game)
            {
                game.suspicion -= 24f;
                game.stock.Heat = Mathf.Max(0f, game.stock.Heat - 18f);
                game.BlockBuilding(BuildingType.FireBarrel, 24f);
                game.Notify("Подозрение резко упало, тепло тоже недовольно.");
            }),
            new EventChoice("Назвать это арт-инсталляцией", "<color=#9cff93>50%: +5 монеток</color> | <color=#ff8f7a>50%: +18 подозрения</color>", delegate(BridgeGrandpasGame game)
            {
                if (game.Roll(0.5f))
                {
                    game.stock.Coins += 5f;
                    game.Notify("+5 монеток. Инспектор позвал знакомого куратора.");
                }
                else
                {
                    game.suspicion += 18f;
                    game.Notify("Инсталляция не прошла согласование.");
                }
            })));

        events.Add(new BridgeEvent(
            "Бездомный чайник с характером",
            "К коммуне прикатился чайник. Он ничего не говорит, но осуждает.",
            new EventChoice("Поставить к самовару", "<color=#9cff93>+18 чая</color>", delegate(BridgeGrandpasGame game)
            {
                game.stock.Tea += 18f;
                game.Notify("+18 чая. Чайник принят как младший родственник.");
            }),
            new EventChoice("Объявить его святыней", "<color=#9cff93>+14 ворчания</color> | <color=#ff8f7a>+5 подозрения</color>", delegate(BridgeGrandpasGame game)
            {
                game.stock.Grumble += 14f;
                game.suspicion += 5f;
                game.Notify("+14 ворчания. Святыня немного блестит на виду.");
            })));

        events.Add(new BridgeEvent(
            "Ночной рынок у лестницы",
            "Кто-то продаёт батарейки, носки и почти новый кусок забора.",
            new EventChoice("Купить батарейки для радио", "<color=#ffcf7a>если есть 2 монетки: -2 монетки</color> | <color=#9cff93>следующий слух быстрее</color>", delegate(BridgeGrandpasGame game)
            {
                if (game.stock.Coins >= 2f)
                {
                    game.stock.Coins -= 2f;
                    game.nextEventIn = Mathf.Min(game.nextEventIn, 8f);
                    game.Notify("Следующий городской слух придёт быстрее.");
                }
                else
                {
                    game.Notify("Монеток не хватило. Продавец понимающе зашуршал.");
                }
            }),
            new EventChoice("Обменять байку на картон", "<color=#9cff93>+18 картона</color> | <color=#ffcf7a>-8 ворчания</color>", delegate(BridgeGrandpasGame game)
            {
                game.stock.Grumble = Mathf.Max(0f, game.stock.Grumble - 8f);
                game.stock.Cardboard += 18f;
                game.Notify("+18 картона, -8 ворчания. Байка ушла в народ.");
            }),
            new EventChoice("Просто поторговаться", "<color=#9cff93>+1 монетка</color> | <color=#9cff93>+8 ворчания</color>", delegate(BridgeGrandpasGame game)
            {
                game.stock.Coins += 1f;
                game.stock.Grumble += 8f;
                game.Notify("+1 монетка и +8 ворчания. Торг был красивый.");
            })));

        events.Add(new BridgeEvent(
            "Дождь усилился",
            "Вода барабанит по мосту, но под ним стало уютнее и страннее.",
            new EventChoice("Собирать воду для самовара", "<color=#9cff93>+12 чая</color> | <color=#ffcf7a>-5 тепла</color>", delegate(BridgeGrandpasGame game)
            {
                game.stock.Tea += 12f;
                game.stock.Heat = Mathf.Max(0f, game.stock.Heat - 5f);
                game.Notify("+12 чая, -5 тепла. Физика чайной жизни сурова.");
            }),
            new EventChoice("Утеплить картоном", "<color=#9cff93>если есть 8 картона: +18 тепла</color> | <color=#ffcf7a>иначе: +4 тепла</color>", delegate(BridgeGrandpasGame game)
            {
                if (game.stock.Cardboard >= 8f)
                {
                    game.stock.Cardboard -= 8f;
                    game.stock.Heat += 18f;
                    game.Notify("-8 картона, +18 тепла.");
                }
                else
                {
                    game.stock.Heat += 4f;
                    game.Notify("Картона мало, но дедушки нашли общий шарф.");
                }
            })));

        events.Add(new BridgeEvent(
            "Радио ловит странную частоту",
            "Сквозь шипение слышно: \"комиссия... ковры... вторник...\"",
            new EventChoice("Записать предупреждение", "<color=#9cff93>-14 подозрения</color> | <color=#9cff93>+6 ворчания</color>", delegate(BridgeGrandpasGame game)
            {
                game.suspicion -= 14f;
                game.stock.Grumble += 6f;
                game.Notify("Подозрение снижено. Радио звучит самодовольно.");
            }),
            new EventChoice("Попросить прогноз погоды", "<color=#9cff93>+12 тепла</color> | <color=#9cff93>+8 чая</color>", delegate(BridgeGrandpasGame game)
            {
                game.stock.Heat += 12f;
                game.stock.Tea += 8f;
                game.Notify("+12 тепла и +8 чая. Прогноз был душевный.");
            }),
            new EventChoice("Настроить на философию", "<color=#9cff93>25%: дед-радиоприёмник</color> | <color=#9cff93>иначе: +16 ворчания</color>", delegate(BridgeGrandpasGame game)
            {
                if (game.Roll(0.25f))
                {
                    game.SpawnGrandpa(GrandpaRole.RadioReceiver, game.RandomSpawnPosition());
                    game.rareMutationSeen = true;
                    game.Notify("Из помех вышел дед-радиоприёмник. Никто не удивился достаточно быстро.");
                }
                else
                {
                    game.stock.Grumble += 16f;
                    game.Notify("+16 ворчания. Частота оказалась круглым столом.");
                }
            })));
    }

}

