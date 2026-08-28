namespace FeatureFlag.Web.Services;

public record EBirdApiResponse(
    string SpeciesCode,
    string ComName,
    string SciName,
    string LocId,
    string LocName,
    string ObsDt,
    int HowMany,
    double Lat,
    double Lng,
    bool ObsValid,
    bool ObsReviewed,
    bool LocationPrivate,
    string SubId
);