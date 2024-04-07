using GTFS.Entities;

namespace AtacFeed
{
    public class LineaAgenzia(Route route, Agency agency)
    {
        public Route Route { get; set; } = route;
        public Agency Agency { get; set; } = agency;
    }
}
