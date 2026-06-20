/// <summary>
/// Estado temporario partilhado pelas tres paginas da UI. Os valores existem
/// apenas durante a execucao e nao sao guardados numa base de dados ou ficheiro.
/// </summary>
public static class UserSelectionData
{
    public static BaseApartmentData SelectedApartment;
    public static FloorPlanRequestData UserRequest;
    public static string UserRequirementsJson;
}
