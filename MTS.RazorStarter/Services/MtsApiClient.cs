using System.Net.Http.Json;

public class MtsApiClient
{
    private readonly HttpClient _http;

    public MtsApiClient(HttpClient http)
    {
        _http = http;
    }

    // =========================================================
    // SYSTEM
    // =========================================================

    public async Task<bool> HealthCheck()
    {
        var res = await _http.GetAsync("/api/system/preflight");
        return res.IsSuccessStatusCode;
    }

    public async Task Bootstrap()
    {
        var res = await _http.PostAsync("/api/system/bootstrap", null);
        res.EnsureSuccessStatusCode();
    }

    // =========================================================
    // PRODUCTION (CORE)
    // =========================================================

    public async Task MovePallet(int palletId, string destination)
    {
        var content = Form(new()
        {
            ["destination"] = destination
        });

        var res = await _http.PostAsync($"/production/pallet/{palletId}/move", content);
        res.EnsureSuccessStatusCode();
    }

    public async Task CreatePallet(Dictionary<string, string> data)
    {
        var res = await _http.PostAsync("/production/create-pallet", Form(data));
        res.EnsureSuccessStatusCode();
    }

    public async Task DeletePallet(int palletId)
    {
        var res = await _http.PostAsync($"/production/pallet/{palletId}/delete", null);
        res.EnsureSuccessStatusCode();
    }

    public async Task ReleaseToQueue(int palletId)
    {
        var res = await _http.PostAsync($"/production/pallet/{palletId}/release", null);
        res.EnsureSuccessStatusCode();
    }

    public async Task ApproveQuality(int palletId)
    {
        var res = await _http.PostAsync($"/production/quality-control/{palletId}/approve", null);
        res.EnsureSuccessStatusCode();
    }

    public async Task RejectQuality(int palletId)
    {
        var res = await _http.PostAsync($"/production/quality-control/{palletId}/reject", null);
        res.EnsureSuccessStatusCode();
    }

    // =========================================================
    // ENGINEERING
    // =========================================================

    public async Task CreatePart(Dictionary<string, string> data)
    {
        var res = await _http.PostAsync("/engineering/parts", Form(data));
        res.EnsureSuccessStatusCode();
    }

    public async Task DeletePart(string partId)
    {
        var res = await _http.PostAsync($"/engineering/parts/{partId}/delete", null);
        res.EnsureSuccessStatusCode();
    }

    public async Task CreateRevision(string partId)
    {
        var res = await _http.PostAsync($"/engineering/parts/{partId}/revisions/create", null);
        res.EnsureSuccessStatusCode();
    }

    // =========================================================
    // IMPORTS (YOUR PDF PIPELINE)
    // =========================================================

    public async Task<HttpResponseMessage> ParseFrame(Stream fileStream, string fileName)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);

        return await _http.PostAsync("/imports/frame/parse", content);
    }

    public async Task<HttpResponseMessage> ParseCutSheet(Stream fileStream, string fileName)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);

        return await _http.PostAsync("/imports/cutsheet/parse", content);
    }

    public async Task CommitCutSheet(Dictionary<string, string> data)
    {
        var res = await _http.PostAsync("/imports/cutsheet/commit", Form(data));
        res.EnsureSuccessStatusCode();
    }

    public async Task CommitFrame(Dictionary<string, string> data)
    {
        var res = await _http.PostAsync("/imports/frame/commit", Form(data));
        res.EnsureSuccessStatusCode();
    }

    // =========================================================
    // SAMBA (YOUR FILE SHARE CONTROL)
    // =========================================================

    public async Task SyncSambaUsers()
    {
        var res = await _http.PostAsync("/api/samba/sync-users", null);
        res.EnsureSuccessStatusCode();
    }

    public async Task ApplySambaShares()
    {
        var res = await _http.PostAsync("/api/samba/apply-shares", null);
        res.EnsureSuccessStatusCode();
    }

    public async Task ApplySambaConfig()
    {
        var res = await _http.PostAsync("/api/samba/apply", null);
        res.EnsureSuccessStatusCode();
    }

    // =========================================================
    // DOCUMENTS
    // =========================================================

    public async Task<List<T>> GetDocuments<T>(string ownerType, int ownerId)
    {
        return await _http.GetFromJsonAsync<List<T>>(
            $"/api/documents?owner_type={ownerType}&owner_id={ownerId}"
        );
    }

    // =========================================================
    // STATIONS
    // =========================================================

    public async Task<T> GetStationLogs<T>(int stationId)
    {
        return await _http.GetFromJsonAsync<T>(
            $"/api/stations/{stationId}/logs"
        );
    }

    public async Task<string> GetScreenStreamUrl(int stationId)
    {
        var res = await _http.GetFromJsonAsync<dynamic>(
            $"/api/stations/{stationId}/screenstream/url"
        );

        return res?.url;
    }

    // =========================================================
    // MACHINE PRESETS
    // =========================================================

    public async Task<List<T>> GetMachinePresets<T>()
    {
        return await _http.GetFromJsonAsync<List<T>>("/api/machine-presets/");
    }

    public async Task CreateMachinePreset(Dictionary<string, string> data)
    {
        var res = await _http.PostAsync("/api/machine-presets/", Form(data));
        res.EnsureSuccessStatusCode();
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private FormUrlEncodedContent Form(Dictionary<string, string> data)
    {
        return new FormUrlEncodedContent(data);
    }
}