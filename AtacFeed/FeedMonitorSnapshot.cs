using System;
using System.Collections.Generic;

namespace AtacFeed
{
    /// <summary>
    /// Snapshot dei dati necessari all'interfaccia per rappresentare lo stato del monitoraggio.
    /// Non espone manager, controlli UI o tipi specifici di WinForms/GTFS.
    /// </summary>
    public sealed class FeedMonitorSnapshot
    {
        public int NumeroLetture { get; set; }
        public int NumeroFeedValidi { get; set; }
        public DateTime? UltimoFeed { get; set; }
        public int CodiceFeedVeicoli { get; set; }
        public int CodiceFeedAvvisi { get; set; }
        public bool MonitoraggioAttivo { get; set; }

        public int TotaleRighe { get; set; }
        public int TotaleIdVettura { get; set; }
        public int TotaleMatricola { get; set; }
        public int TotaleMatricolaAtac { get; set; }
        public int TotaleMatricolaTpl { get; set; }
        public int TotaleVettureAtac { get; set; }
        public int TotaleVettureTpl { get; set; }
        public int TotaleVetture { get; set; }
        public double PonderateAtac { get; set; }
        public double PonderateTpl { get; set; }

        public FeedStatisticsDto Statistiche { get; set; }

        public IList<VehicleSnapshotDto> VeicoliAttuali { get; set; } = new List<VehicleSnapshotDto>();
        public IList<VehicleSnapshotDto> VeicoliAggregati { get; set; } = new List<VehicleSnapshotDto>();
        public IList<VehicleSnapshotDto> VeicoliAggiunti { get; set; } = new List<VehicleSnapshotDto>();
        public IList<VehicleSnapshotDto> VeicoliRimossi { get; set; } = new List<VehicleSnapshotDto>();

        public IList<FeedIssueDto> ProblemiFeed { get; set; } = new List<FeedIssueDto>();
        public IList<VehicleAnomalyDto> AnomalieGtfs { get; set; } = new List<VehicleAnomalyDto>();
        public IList<FeedNoticeDto> Avvisi { get; set; } = new List<FeedNoticeDto>();
        public IList<AlertRuleGroupDto> GruppiAlert { get; set; } = new List<AlertRuleGroupDto>();
        public IList<MonitorViolationDto> ViolazioniMonitoraggio { get; set; } = new List<MonitorViolationDto>();
        public IList<OvercrowdingDto> VettureSovraffollate { get; set; } = new List<OvercrowdingDto>();
        public IList<MonitorChartPointDto> PuntiGrafico { get; set; } = new List<MonitorChartPointDto>();
        public IList<ServiceStatisticDto> ServiziPerGestore { get; set; } = new List<ServiceStatisticDto>();
    }

    public sealed class FeedMonitorSnapshotEventArgs : EventArgs
    {
        public FeedMonitorSnapshot Snapshot { get; private set; }

        public FeedMonitorSnapshotEventArgs(FeedMonitorSnapshot snapshot)
        {
            Snapshot = snapshot;
        }
    }

    /// <summary>
    /// Configurazione caricata per la presentazione iniziale, senza dipendenze da GTFS o WinForms.
    /// </summary>
    public sealed class FeedConfigurationDto
    {
        public IList<RouteOptionDto> LineeDisponibili { get; set; } = new List<RouteOptionDto>();
        public IList<AgencyRouteDto> LineePerGestore { get; set; } = new List<AgencyRouteDto>();
        public IList<AgencyDistributionDto> DistribuzioneGestori { get; set; } = new List<AgencyDistributionDto>();
        public IList<MonitorViolationDto> RegoleMonitoraggio { get; set; } = new List<MonitorViolationDto>();
        public IList<AlertRuleGroupDto> GruppiAlert { get; set; } = new List<AlertRuleGroupDto>();
        public int EsitoCriteriMediaPonderata { get; set; }
        public bool LetturaAlertRiuscita { get; set; }
    }

    public sealed class RouteOptionDto
    {
        public string Id { get; set; }
        public string NomeVisualizzato { get; set; }
    }

    public sealed class AgencyRouteDto
    {
        public string Gestore { get; set; }
        public string Linea { get; set; }
        public int CodiceTipoTrasporto { get; set; }        
        public string TipoTrasporto { get; set; }
    }

    public sealed class AgencyDistributionDto
    {
        public string Gestore { get; set; }
        public int NumeroCorse { get; set; }
        public decimal PercentualeCorse { get; set; }
        public int TotaleCorse { get; set; }
    }

    /// <summary>
    /// Dati di una vettura esposti per la presentazione, sia corrente sia aggregata.
    /// Gli stati del feed sono stringhe per non dipendere dagli enum del modello GTFS.
    /// </summary>
    public sealed class VehicleSnapshotDto
    {
        public string IdVettura { get; set; }
        public string Matricola { get; set; }
        public string LicensePlate { get; set; }
        public string RouteId { get; set; }
        public string Linea { get; set; }
        public string Gestore { get; set; }
        public uint? DirectionId { get; set; }
        public string Destinazione { get; set; }
        public uint CurrentStopSequence { get; set; }
        public string CongestionLevel { get; set; }
        public string OccupancyStatus { get; set; }
        public string TripId { get; set; }
        public DateTime PrimaVolta { get; set; }
        public DateTime? UltimaVolta { get; set; }
        public string Rimessa { get; set; }
        public string Euro { get; set; }
        public string Modello { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string InTransitTo { get; set; }
        public int TipoMezzoTrasporto { get; set; }
        public double DistanzaPercorsa { get; set; }
        public string NomeFermata { get; set; }
        public DateTime? PartenzaProgrammata { get; set; }
        public DateTime? PartenzaEffettiva { get; set; }
    }

    public sealed class FeedStatisticsDto
    {
        public int RilevatoBusAtac { get; set; }
        public int RilevatoTramAtac { get; set; }
        public int RilevatoFilobusAtac { get; set; }
        public int RilevatoMinibusElettrici { get; set; }
        public int RilevatoFurgoncini { get; set; }
        public int RilevatoFerro { get; set; }
        public int RilevatoAltroAtac { get; set; }
        public int RilevatoBusTpl { get; set; }
        public int RilevatoPullmanTpl { get; set; }
        public int RilevatoAltroTpl { get; set; }
    }

    public sealed class ServiceStatisticDto
    {
        public string Gestore { get; set; }
        public string Servizio { get; set; }
        public int Numero { get; set; }
    }

    public sealed class FeedIssueDto
    {
        public string Categoria { get; set; }
        public string Messaggio { get; set; }
        public string Dettaglio { get; set; }
    }

    public sealed class VehicleAnomalyDto
    {
        public string Categoria { get; set; }
        public string Matricola { get; set; }
        public string Linea { get; set; }
        public DateTime OraViolazione { get; set; }
        public string TripId { get; set; }
        public uint Fermata { get; set; }
        public int Delta { get; set; }
    }

    public sealed class FeedNoticeDto
    {
        public string Id { get; set; }
        public DateTime? ValidoDal { get; set; }
        public DateTime? ValidoAl { get; set; }
        public string LineeCoinvolte { get; set; }
        public string Titolo { get; set; }
        public string Descrizione { get; set; }
        public string Cause { get; set; }
        public string Effect { get; set; }
    }

    public sealed class AlertRuleGroupDto
    {
        public string Nome { get; set; }
        public IList<AlertViolationDto> Regole { get; set; } = new List<AlertViolationDto>();
        public IList<AlertViolationDto> ViolazioniStoriche { get; set; } = new List<AlertViolationDto>();
        public IList<AlertViolationDto> ViolazioniAttuali { get; set; } = new List<AlertViolationDto>();
    }

    public sealed class AlertViolationDto
    {
        public string Linea { get; set; }
        public string Giorno { get; set; }
        public TimeSpan? Da { get; set; }
        public TimeSpan? A { get; set; }
        public string VetturaDa { get; set; }
        public string VetturaA { get; set; }
        public string Violazione { get; set; }
        public DateTime? OraPrimaViolazione { get; set; }
        public DateTime? OraUltimaViolazione { get; set; }
    }

    public sealed class MonitorViolationDto
    {
        public string Linea { get; set; }
        public string Giorno { get; set; }
        public TimeSpan Da { get; set; }
        public TimeSpan? A { get; set; }
        public int TempoBonus { get; set; }
        public int? VetturePreviste { get; set; }
        public int? VettureRilevate { get; set; }
        public DateTime? OraPrimaViolazione { get; set; }
        public DateTime? OraUltimaViolazione { get; set; }
    }

    public sealed class OvercrowdingDto
    {
        public string TripId { get; set; }
        public string Linea { get; set; }
        public string Matricola { get; set; }
        public string OccupancyStatus { get; set; }
        public uint PrimaFermata { get; set; }
        public DateTime? PrimaVolta { get; set; }
        public uint? UltimaFermata { get; set; }
        public DateTime? UltimaVolta { get; set; }
    }

    public sealed class MonitorChartPointDto
    {
        public DateTime DataOra { get; set; }
        public int TotaleAggregato { get; set; }
        public int AggregatoAtac { get; set; }
        public int AggregatoTpl { get; set; }
        public int Atac { get; set; }
        public int Tpl { get; set; }
        public int Rilevate { get; set; }
        public int Aggiunte { get; set; }
        public int Rimosse { get; set; }
    }
}
