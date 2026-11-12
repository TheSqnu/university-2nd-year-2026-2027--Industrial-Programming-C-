namespace Hospital.Business;

public enum Gender { Male, Female }

public enum DosageForm { Injection, Tablet }

public enum PrescriptionKind { Medication, Diagnostic, Preventive }

public static class Labels
{
    public static string ToRu(this Gender g) => g == Gender.Male ? "Мужской" : "Женский";

    public static string ToRu(this PrescriptionKind k) => k switch
    {
        PrescriptionKind.Medication => "Медикаментозное",
        PrescriptionKind.Diagnostic => "Диагностическое",
        _ => "Профилактическое"
    };
}
