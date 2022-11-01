using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtacFeed
{
    internal class FeedAlertManager: BaseFeedManager
    {
        public List<Avviso> Avvisi => LastValidFeed.Entities
                                        .Where(x => !x.IsDeleted)
                                        .Select(x => new Avviso(
                                            id: x.Id,
                                            validoDal: t0.AddSeconds(x.Alert.ActivePeriods.FirstOrDefault()?.Start ?? 0).ToLocalTime(),
                                            validoAl: t0.AddSeconds(x.Alert.ActivePeriods.FirstOrDefault()?.End ?? 0).ToLocalTime(),
                                            titolo: string.Join(Environment.NewLine, x.Alert.HeaderText.Translations.Select(i => i.Text)),
                                            descrizione: string.Join(Environment.NewLine, x.Alert.DescriptionText.Translations.Select(i => i.Text)),
                                            lineeCoinvolte: string.Join(", " + Environment.NewLine, x.Alert.InformedEntities.Select(i => i.RouteId)),
                                            cause: x.Alert.cause.ToString(),
                                            effect: x.Alert.effect.ToString()))
                                        .ToList();
    }
}
