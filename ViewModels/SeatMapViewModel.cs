using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.ViewModels
{
    public class SeatMapViewModel
    {
        public Resource? Resource { get; set; }
        public IEnumerable<Seat>? Seats { get; set; }
    }

    public class SeatMapBuilderViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public int SeatMapWidth { get; set; }
        public int SeatMapHeight { get; set; }
        public List<SeatDto> Seats { get; set; } = new();
    }

    public class SeatDto
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Label { get; set; } = "";
        public string Status { get; set; } = "available";
    }
}
