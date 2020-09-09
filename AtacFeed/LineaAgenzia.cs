using GTFS.Entities;

namespace AtacFeed
{
    public class LineaAgenzia
    {
        public Route Route { get; set; }
        public Agency Agency { get; set; }

        public LineaAgenzia(Route route, Agency agency)
        {
            Route = route;
            Agency = agency;
        }
    }
}
