using System.Globalization;
using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private ResourceStock CurrentResourceIncomePerSecond()
    {
        float common = CountRole(GrandpaRole.Common);
        float samovar = CountRole(GrandpaRole.SamovarKeeper);
        float cardboarder = CountRole(GrandpaRole.Cardboarder);
        float mutterer = CountRole(GrandpaRole.Mutterer);
        float philosopher = CountRole(GrandpaRole.Philosopher);
        ResourceStock income = new ResourceStock(0f, 0f, 0f, 0f, 0f);

        Building fire = buildings[BuildingType.FireBarrel];
        if (fire.Built && !fire.IsBlocked)
        {
            income.Heat += 0.23f + fire.Level * 0.11f;
        }

        Building samovarBuilding = buildings[BuildingType.Samovar];
        if (samovarBuilding.Built && !samovarBuilding.IsBlocked)
        {
            income.Tea += 0.16f + samovarBuilding.Level * 0.08f + samovar * 0.11f;
        }

        Building bench = buildings[BuildingType.GrumbleBench];
        if (bench.Built && !bench.IsBlocked)
        {
            income.Grumble += 0.11f + bench.Level * 0.06f + mutterer * 0.14f + philosopher * 0.05f;
        }

        income.Grumble += common * 0.035f + mutterer * 0.05f;
        income.Cardboard += common * 0.025f + cardboarder * 0.12f;

        if (buildings[BuildingType.RadioMayak].Built && !buildings[BuildingType.RadioMayak].IsBlocked)
        {
            income.Coins += CountRole(GrandpaRole.RadioReceiver) * 0.012f;
        }

        float cozyMultiplier = CozyIncomeMultiplier();
        income.Tea *= cozyMultiplier;
        income.Heat *= cozyMultiplier;
        income.Cardboard *= cozyMultiplier;
        income.Grumble *= cozyMultiplier;
        return income;
    }

    private string BuildTopResourceStats()
    {
        ResourceStock income = CurrentResourceIncomePerSecond();
        return ResourceStat("Чай", stock.Tea, income.Tea) +
            "   " + ResourceStat("Тепло", stock.Heat, income.Heat) +
            "   " + ResourceStat("Картон", stock.Cardboard, income.Cardboard) +
            "   " + ResourceStat("Ворчание", stock.Grumble, income.Grumble) +
            "   " + ResourceStat("Монетки", stock.Coins, income.Coins) +
            "   " + CozyStat();
    }

    private string ResourceStat(string label, float amount, float perSecond)
    {
        float perMinute = perSecond * 60f;
        string color = Mathf.Abs(perMinute) < 0.05f ? "#6f7a86" : "#9cff93";
        return label + " " + F(amount) + " <color=" + color + ">+" + RateF(perMinute) + "/м</color>";
    }

    private string RateF(float perMinute)
    {
        if (Mathf.Abs(perMinute) < 0.05f)
        {
            return "0";
        }

        string format = Mathf.Abs(perMinute) < 10f ? "0.#" : "0";
        return perMinute.ToString(format, CultureInfo.InvariantCulture);
    }
}
