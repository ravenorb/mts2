using MTS.RazorStarter.Models;

namespace MTS.RazorStarter.Services;

public interface IMtsApiClient
{
    Task<ApiHealthSummary> GetHealthAsync(CancellationToken cancellationToken = default);
    Task<PalletMoveResult> MovePalletAsync(PalletMoveRequest request, CancellationToken cancellationToken = default);
}
