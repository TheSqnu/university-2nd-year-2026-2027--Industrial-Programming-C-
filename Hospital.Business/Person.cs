using System.Text.Json.Serialization;

namespace Hospital.Business;

public abstract class Person : BusinessObject
{
    public string LastName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string Patronymic { get; set; } = "";

    [JsonIgnore]
    public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();

    protected void ValidatePerson()
    {
        Require(!string.IsNullOrWhiteSpace(LastName), nameof(LastName), "Укажите фамилию.");
        Require(!string.IsNullOrWhiteSpace(FirstName), nameof(FirstName), "Укажите имя.");
    }
}
