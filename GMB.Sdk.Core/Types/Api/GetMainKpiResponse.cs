using GMB.Sdk.Core.Types.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public class GetMainKpiResponse : GenericResponse<GetMainKpiResponse>
    {
        public GetMainKpiResponse(MainKPI? kpi)
        {
            Kpi = kpi;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetMainKpiResponse() { }

        public MainKPI? Kpi { get; set; }

    }
}
