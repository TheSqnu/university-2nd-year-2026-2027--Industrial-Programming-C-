namespace Hospital.Business;

public class Patient : Person
{
    public DateTime BirthDate { get; set; } = DateTime.Today.AddYears(-30);
    public Gender Gender { get; set; }
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";

    protected override void ValidateRules()
    {
        ValidatePerson();
        Require(BirthDate.Date <= DateTime.Today, nameof(BirthDate), "Дата рождения не может быть в будущем.");
        Require(BirthDate.Year >= 1900, nameof(BirthDate), "Дата рождения не может быть раньше 1900 года.");
        if (!string.IsNullOrWhiteSpace(Phone))
            Require(Phone.Count(char.IsDigit) >= 5, nameof(Phone), "Телефон должен содержать не менее 5 цифр.");
    }

    public override string ToString() => FullName;
}
