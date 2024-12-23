using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtacFeed
{
    internal class FeedAlertManager : BaseFeedManager
    {
        public List<Avviso> Avvisi => LastValidFeed.Entities
                                        .Where(x => !x.IsDeleted)
                                        .Select(x => new Avviso(
                                            id: x.Id,
                                            validoDal: t0.AddSeconds(x.Alert?.ActivePeriods?.FirstOrDefault()?.Start ?? 0).ToLocalTime(),
                                            validoAl: t0.AddSeconds(x.Alert?.ActivePeriods?.FirstOrDefault()?.End ?? 0).ToLocalTime(),
                                            titolo: string.Join(Environment.NewLine, x.Alert?.HeaderText?.Translations?.Select(i => i.Text) ?? new List<string>() {""}),
                                            descrizione: string.Join(Environment.NewLine, x.Alert?.DescriptionText?.Translations?.Select(i => i.Text) ?? new List<string>() { "" }),
                                            lineeCoinvolte: string.Join(", " + Environment.NewLine, x.Alert?.InformedEntities?.Select(i => i.RouteId) ?? new List<string>() { "" }),
                                            cause: x.Alert?.cause.ToString(),
                                            effect: x.Alert?.effect.ToString()))
                                        .ToList()
                                    ?? null;
        public bool DiversoDaPrecedente => LastValidFeed?.Entities.Any(p => !(PrevValidFeed?.Entities.Any(p2 => p2.Id == p.Id) ?? false)) ?? true;

    }
}
