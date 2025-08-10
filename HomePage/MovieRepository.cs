namespace HomePage
{
    public class MovieRepository : Repository<Movie>
    {
        public override string FileName => "Movie.txt";
    }
}
