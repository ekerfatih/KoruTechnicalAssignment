namespace KoruTechnicalAssignment.Domain.Entities.Db {
    public class Branch : Entity{
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public ICollection<Request> Requests { get; set; } = new List<Request>();
    }
}
