namespace UniversalReservationMVC.ViewModels
{
    public class SelectedSeatViewModel
{
    public string ResourceName { get; set; } = string.Empty; // np. nazwa sali/zasobu
    public int X { get; set; } // rząd
    public int Y { get; set; } // miejsce
}
}
