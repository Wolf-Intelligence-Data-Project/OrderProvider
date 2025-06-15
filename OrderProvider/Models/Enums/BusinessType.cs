using System.ComponentModel;

namespace OrderProvider.Models.Enums;

// Custom made ENUM with SNI codes

public enum BusinessType
{
    [Description("Jordbruk och jakt samt service i anslutning härtill")]
    A01 = 01,

    [Description("Skogsbruk")]
    A02 = 02,

    [Description("Fiske och vattenbruk")]
    A03 = 03,

    [Description("Kolutvinning")]
    B05 = 05,

    [Description("Utvinning av råpetroleum och naturgas")]
    B06 = 06,

    [Description("Utvinning av metallmalmer")]
    B07 = 07,

    [Description("Annan utvinning av mineral")]
    B08 = 08,

    [Description("Service till utvinning")]
    B09 = 09,

    [Description("Livsmedelsframställning")]
    C10 = 10,

    [Description("Framställning av drycker")]
    C11 = 11,

    [Description("Tobaksvarutillverkning")]
    C12 = 12,

    [Description("Textilvarutillverkning")]
    C13 = 13,

    [Description("Tillverkning av kläder")]
    C14 = 14,

    [Description("Tillverkning av läder, läder- och skinnvaror m.m.")]
    C15 = 15,

    [Description("Tillverkning av trä och varor av trä, kork, rotting o.d. utom möbler")]
    C16 = 16,

    [Description("Pappers- och pappersvarutillverkning")]
    C17 = 17,

    [Description("Grafisk produktion och reproduktion av inspelningar")]
    C18 = 18,

    [Description("Tillverkning av stenkolsprodukter och raffinerade petroleumprodukter")]
    C19 = 19,

    [Description("Tillverkning av kemikalier och kemiska produkter")]
    C20 = 20,

    [Description("Tillverkning av farmaceutiska basprodukter och läkemedel")]
    C21 = 21,

    [Description("Tillverkning av gummi- och plastvaror")]
    C22 = 22,

    [Description("Tillverkning av andra icke-metalliska mineraliska produkter")]
    C23 = 23,

    [Description("Stål- och metallframställning")]
    C24 = 24,

    [Description("Tillverkning av metallvaror utom maskiner och apparater")]
    C25 = 25,

    [Description("Tillverkning av datorer, elektronikvaror och optik")]
    C26 = 26,

    [Description("Tillverkning av elapparatur")]
    C27 = 27,

    [Description("Tillverkning av övriga maskiner")]
    C28 = 28,

    [Description("Tillverkning av motorfordon, släpfordon och påhängsvagnar")]
    C29 = 29,

    [Description("Tillverkning av andra transportmedel")]
    C30 = 30,

    [Description("Tillverkning av möbler")]
    C31 = 31,

    [Description("Annan tillverkning")]
    C32 = 32,

    [Description("Reparation och installation av maskiner och apparater")]
    C33 = 33,

    [Description("Försörjning av el, gas, värme och kyla")]
    D35 = 35,

    [Description("Vattenförsörjning")]
    E36 = 36,

    [Description("Avloppsrening")]
    E37 = 37,

    [Description("Avfallshantering; återvinning")]
    E38 = 38,

    [Description("Sanering, efterbehandling av jord och vatten samt annan verksamhet för föroreningsbekämpning")]
    E39 = 39,

    [Description("Byggande av hus")]
    F41 = 41,

    [Description("Anläggningsarbeten")]
    F42 = 42,

    [Description("Specialiserad bygg- och anläggningsverksamhet")]
    F43 = 43,

    [Description("Handel samt reparation av motorfordon och motorcyklar")]
    G45 = 45,

    [Description("Parti- och provisionshandel utom med motorfordon")]
    G46 = 46,

    [Description("Detaljhandel utom med motorfordon och motorcyklar")]
    G47 = 47,

    [Description("Landtransport; transport i rörsystem")]
    H49 = 49,

    [Description("Sjötransport")]
    H50 = 50,

    [Description("Lufttransport")]
    H51 = 51,

    [Description("Magasinering och stödtjänster till transport")]
    H52 = 52,

    [Description("Post- och kurirverksamhet")]
    H53 = 53,

    [Description("Hotell- och logiverksamhet")]
    I55 = 55,

    [Description("Restaurang-, catering- och barverksamhet")]
    I56 = 56,

    [Description("Förlagsverksamhet")]
    J58 = 58,

    [Description("Film-, video- och tv-programverksamhet, ljudinspelningar och fonogramutgivning")]
    J59 = 59,

    [Description("Planering och sändning av program")]
    J60 = 60,

    [Description("Telekommunikation")]
    J61 = 61,

    [Description("Dataprogrammering, datakonsultverksamhet o.d.")]
    J62 = 62,

    [Description("Informationstjänster")]
    J63 = 63,

    [Description("Finansiella tjänster utom försäkring och pensionsfondsverksamhet")]
    K64 = 64,

    [Description("Försäkring, återförsäkring och pensionsfondsverksamhet utom obligatorisk socialförsäkring")]
    K65 = 65,

    [Description("Stödtjänster till finansiella tjänster och försäkring")]
    K66 = 66,

    [Description("Fastighetsverksamhet")]
    L68 = 68,

    [Description("Juridisk och ekonomisk konsultverksamhet")]
    M69 = 69,

    [Description("Verksamheter som utövas av huvudkontor; konsulttjänster till företag")]
    M70 = 70,

    [Description("Arkitekt- och teknisk konsultverksamhet; teknisk provning och analys")]
    M71 = 71,

    [Description("Vetenskaplig forskning och utveckling")]
    M72 = 72,

    [Description("Reklam och marknadsundersökning")]
    M73 = 73,

    [Description("Annan verksamhet inom juridik, ekonomi, vetenskap och teknik")]
    M74 = 74,

    [Description("Veterinärverksamhet")]
    M75 = 75,

    [Description("Uthyrning och leasing")]
    N77 = 77,

    [Description("Arbetsförmedling, bemanning och andra personalrelaterade tjänster")]
    N78 = 78,

    [Description("Resebyrå- och researrangörsverksamhet och andra resetjänster och relaterade tjänster")]
    N79 = 79,

    [Description("Säkerhets- och bevakningsverksamhet")]
    N80 = 80,

    [Description("Fastighetsservice samt skötsel och underhåll av grönytor")]
    N81 = 81,

    [Description("Kontorstjänster och andra företagstjänster")]
    N82 = 82,

    [Description("Offentlig förvaltning och försvar; obligatorisk socialförsäkring")]
    O84 = 84,

    [Description("Utbildning")]
    P85 = 85,

    [Description("Hälso- och sjukvård")]
    Q86 = 86,

    [Description("Vård och omsorg med boende")]
    Q87 = 87,

    [Description("Öppna sociala insatser")]
    Q88 = 88,

    [Description("Konstnärlig och kulturell verksamhet samt underhållningsverksamhet")]
    R90 = 90,

    [Description("Biblioteks-, arkiv- och museiverksamhet m.m.")]
    R91 = 91,

    [Description("Spel- och vadhållningsverksamhet")]
    R92 = 92,

    [Description("Sport-, fritids- och nöjesverksamhet")]
    R93 = 93,

    [Description("Intressebevakning; religiös verksamhet")]
    S94 = 94,

    [Description("Reparation av datorer, hushållsartiklar och personliga artiklar")]
    S95 = 95,

    [Description("Andra konsumenttjänster")]
    S96 = 96,

    [Description("Förvärvsarbete i hushåll")]
    T97 = 97,

    [Description("Hushållens produktion av diverse varor och tjänster för eget bruk")]
    T98 = 98,

    [Description("Verksamhet vid internationella organisationer, utländska ambassader o.d.")]
    U99 = 99
}
