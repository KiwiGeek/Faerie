using Faerie.Building;
using Faerie.Model;

namespace Faerie.Samples.Softporn;

internal sealed partial class SoftpornWorld
{
    private void DefineThings()
    {
        Scenery(SoftpornIds.Desk, _b.Scenery("desk").Called("desk", "drawer"));
        Scenery(SoftpornIds.Washbasin, _b.Scenery("washbasin").Called("washbasin", "basin", "wash"));
        Scenery(SoftpornIds.Graffiti, _b.Scenery("graffiti").Called("graffiti"));
        Scenery(SoftpornIds.Mirror, _b.Scenery("mirror").Called("mirror"));
        Scenery(SoftpornIds.Toilet, _b.Scenery("toilet").Called("toilet"));
        Npc(SoftpornIds.Businessman, _b.Creature("businessman").Called("businessman", "busi"));
        Scenery(SoftpornIds.Button, _b.Scenery("button").Called("button", "butt"));
        Npc(SoftpornIds.Bartender, _b.Creature("bartender").Called("bartender", "bart"));
        Npc(SoftpornIds.Pimp, _b.Creature("pimp").Called("pimp", "dude"));
        Npc(SoftpornIds.Hooker, _b.Creature("hooker").Called("hooker", "hook"));
        Scenery(SoftpornIds.Billboard, _b.Scenery("billboard").Called("billboard", "bill"));
        Npc(SoftpornIds.Preacher, _b.Creature("preacher").Called("preacher", "prea"));
        Scenery(SoftpornIds.Tv, _b.Scenery("tv").Called("tv", "television"));
        Scenery(SoftpornIds.SlotMachines, _b.Scenery("slot machines").Called("slots", "slot", "machines"));
        Scenery(SoftpornIds.Cards, _b.Scenery("cards").Called("cards", "21", "blackjack"));
        Scenery(SoftpornIds.Ashtray, _b.Scenery("ashtray").Called("ashtray", "asht"));
        Npc(SoftpornIds.Blonde, _b.Creature("blonde").Called("blonde", "volu"));
        Scenery(SoftpornIds.Bed, _b.Scenery("bed").Called("bed"));
        Npc(SoftpornIds.Bum, _b.Creature("bum").Called("bum"));
        Scenery(SoftpornIds.Peephole, _b.Scenery("peephole").Called("peephole", "peep", "hole"));
        Scenery(SoftpornIds.DoorWest, _b.Scenery("door").Called("door", "west door"));
        Npc(SoftpornIds.Waitress, _b.Creature("waitress").Called("waitress", "wait"));
        Scenery(SoftpornIds.Table, _b.Scenery("table").Called("table", "tabl"));
        Scenery(SoftpornIds.Telephone, _b.Scenery("telephone").Called("telephone", "phone", "tele"));
        Scenery(SoftpornIds.Closet, _b.Scenery("closet").Called("closet", "clos").Openable());
        Scenery(SoftpornIds.Sink, _b.Scenery("sink").Called("sink").Switchable());
        Scenery(SoftpornIds.Elevator, _b.Scenery("elevator").Called("elevator", "elev"));
        Npc(SoftpornIds.Dealer, _b.Creature("dealer").Called("dealer", "deal"));
        Scenery(SoftpornIds.Cabinet, _b.Scenery("cabinet").Called("cabinet", "cabi").Openable());
        Scenery(SoftpornIds.Bushes, _b.Scenery("bushes").Called("bushes", "bush"));
        Scenery(SoftpornIds.Tree, _b.Scenery("tree").Called("tree"));
        Scenery(SoftpornIds.Window, _b.Scenery("window").Called("window", "wind"));
        Scenery(SoftpornIds.Sign, _b.Scenery("sign").Called("sign"));
        Scenery(SoftpornIds.Taxi, _b.Scenery("taxi").Called("taxi", "cab"));
        Npc(SoftpornIds.Girl, _b.Creature("girl").Called("girl", "eve"));
        Scenery(SoftpornIds.Curtain, _b.Scenery("curtain").Called("curtain", "curt"));

        Takeable(SoftpornIds.Newspaper, _b.Item("newspaper").Called("newspaper", "news", "paper"));
        Takeable(SoftpornIds.Ring, _b.Item("wedding ring").Called("ring", "wedding ring"));
        Takeable(SoftpornIds.Whiskey, _b.Item("whiskey").Called("whiskey", "whis", "shot").Drinkable());
        Takeable(SoftpornIds.Beer, _b.Item("beer").Called("beer").Drinkable());
        Takeable(SoftpornIds.Hammer, _b.Item("hammer").Called("hammer", "hamm"));
        Takeable(SoftpornIds.Garbage, _b.Item("garbage").Called("garbage", "garb", "trash"));
        Takeable(SoftpornIds.Flowers, _b.Item("flowers").Called("flowers", "flow"));
        Takeable(SoftpornIds.AppleCore, _b.Item("apple core").Called("core", "apple core"));
        Takeable(SoftpornIds.Seeds, _b.Item("seeds").Called("seeds", "seed"));
        Takeable(SoftpornIds.Candy, _b.Item("candy").Called("candy", "cand"));
        Takeable(SoftpornIds.Pills, _b.Item("pills").Called("pills", "pill").Edible());
        Takeable(SoftpornIds.Plant, _b.Item("plant").Called("plant", "plan"));
        Takeable(SoftpornIds.Passcard, _b.Item("passcard").Called("passcard", "pass"));
        Takeable(SoftpornIds.Radio, _b.Item("radio").Called("radio", "radi"));
        Takeable(SoftpornIds.Knife, _b.Item("knife").Called("knife", "knif").Wearable());
        Takeable(SoftpornIds.Magazine, _b.Item("magazine").Called("magazine", "maga", "mag"));
        Takeable(SoftpornIds.Rubber, _b.Item("rubber").Called("rubber", "rubb", "condom").Wearable());
        Takeable(SoftpornIds.Wine, _b.Item("wine").Called("wine").Drinkable());
        Takeable(SoftpornIds.Wallet, _b.Item("wallet").Called("wallet", "wall"));
        Takeable(SoftpornIds.Doll, _b.Item("doll").Called("doll"));
        Takeable(SoftpornIds.Apple, _b.Item("apple").Called("apple", "appl").Edible());
        Takeable(SoftpornIds.Pitcher, _b.Item("pitcher").Called("pitcher", "pitc"));
        Takeable(SoftpornIds.Stool, _b.Item("stool").Called("stool", "stoo"));
        Takeable(SoftpornIds.Rope, _b.Item("rope").Called("rope").Wearable());
        Takeable(SoftpornIds.Rack, _b.Item("display rack").Called("rack", "display"));
        Takeable(SoftpornIds.Mushroom, _b.Item("mushroom").Called("mushroom", "mush").Edible());
        Takeable(SoftpornIds.ControlUnit, _b.Item("remote control").Called("control unit", "remote", "cont", "unit"));
        Takeable(SoftpornIds.Water, _b.Item("water").Called("water", "wate").Drinkable());
    }

    private void PlaceThings()
    {
        T(SoftpornIds.Desk).StartsIn(R(SoftpornIds.Hallway));
        T(SoftpornIds.Washbasin).StartsIn(R(SoftpornIds.Bathroom));
        T(SoftpornIds.Graffiti).StartsIn(R(SoftpornIds.Bathroom));
        T(SoftpornIds.Mirror).StartsIn(R(SoftpornIds.Bathroom));
        T(SoftpornIds.Toilet).StartsIn(R(SoftpornIds.Bathroom));
        T(SoftpornIds.Businessman).StartsIn(R(SoftpornIds.Hallway));
        T(SoftpornIds.Bartender).StartsIn(R(SoftpornIds.Bar));
        T(SoftpornIds.Pimp).StartsIn(R(SoftpornIds.Backroom));
        T(SoftpornIds.Hooker).StartsIn(R(SoftpornIds.HookerBedroom));
        T(SoftpornIds.Billboard).StartsIn(R(SoftpornIds.HookerBalcony));
        T(SoftpornIds.Preacher).StartsIn(R(SoftpornIds.MarriageCenter));
        T(SoftpornIds.Tv).StartsIn(R(SoftpornIds.Backroom));
        T(SoftpornIds.SlotMachines).StartsIn(R(SoftpornIds.Casino));
        T(SoftpornIds.Cards).StartsIn(R(SoftpornIds.TwentyOneRoom));
        T(SoftpornIds.Ashtray).StartsIn(R(SoftpornIds.HotelHallway));
        T(SoftpornIds.Blonde).StartsIn(R(SoftpornIds.HotelDesk));
        T(SoftpornIds.Bed).StartsIn(R(SoftpornIds.HoneymoonSuite));
        T(SoftpornIds.Bum).StartsIn(R(SoftpornIds.DiscoStreet));
        T(SoftpornIds.Peephole).StartsIn(R(SoftpornIds.HoneymoonBalcony));
        T(SoftpornIds.DoorWest).StartsIn(R(SoftpornIds.DiscoEntrance));
        T(SoftpornIds.Waitress).StartsIn(R(SoftpornIds.Disco));
        T(SoftpornIds.Table).StartsIn(R(SoftpornIds.Disco));
        T(SoftpornIds.Telephone).StartsIn(R(SoftpornIds.PhoneBooth));
        T(SoftpornIds.Closet).StartsIn(R(SoftpornIds.LivingRoom));
        T(SoftpornIds.Sink).StartsIn(R(SoftpornIds.Kitchen));
        T(SoftpornIds.Elevator).StartsIn(R(SoftpornIds.HotelDesk));
        T(SoftpornIds.Dealer).StartsIn(R(SoftpornIds.TwentyOneRoom));
        T(SoftpornIds.Cabinet).StartsIn(R(SoftpornIds.Kitchen));
        T(SoftpornIds.Window).StartsIn(R(SoftpornIds.WindowLedge));
        T(SoftpornIds.Sign).StartsIn(R(SoftpornIds.BarStreet));
        T(SoftpornIds.Taxi).StartsIn(R(SoftpornIds.BarStreet));
        T(SoftpornIds.Girl).StartsIn(R(SoftpornIds.Disco));
        T(SoftpornIds.Curtain).StartsIn(R(SoftpornIds.Bar));
        T(SoftpornIds.Candy).StartsIn(R(SoftpornIds.HookerBedroom));
        T(SoftpornIds.Pills).StartsIn(R(SoftpornIds.BrokenRoom));
        T(SoftpornIds.Plant).StartsIn(R(SoftpornIds.Lobby));
        T(SoftpornIds.Radio).StartsIn(R(SoftpornIds.HoneymoonBalcony));
        T(SoftpornIds.Rubber).StartsIn(R(SoftpornIds.Pharmacy));
        T(SoftpornIds.Rack).StartsIn(R(SoftpornIds.Pharmacy));
        T(SoftpornIds.Hammer).StartsIn(R(SoftpornIds.Garden));
        T(SoftpornIds.Garbage).StartsIn(R(SoftpornIds.Dumpster));
        T(SoftpornIds.Flowers).StartsIn(R(SoftpornIds.Hallway));
        T(SoftpornIds.Stool).StartsIn(R(SoftpornIds.Garden));
        T(SoftpornIds.Mushroom).StartsIn(R(SoftpornIds.Garden));

        T(SoftpornIds.Wallet).StartsCarried();

        T(SoftpornIds.Wine).OrderableFrom(T(SoftpornIds.Waitress));
        T(SoftpornIds.Beer).OrderableFrom(T(SoftpornIds.Bartender));
        T(SoftpornIds.Whiskey).OrderableFrom(T(SoftpornIds.Bartender));
    }

    private void Scenery(string id, Thing thing)
    {
        thing.Proper();
        _things[id] = thing;
    }

    private void Npc(string id, Thing thing)
    {
        thing.Proper();
        _things[id] = thing;
    }

    private void Takeable(string id, Thing thing) => _things[id] = thing;
}
