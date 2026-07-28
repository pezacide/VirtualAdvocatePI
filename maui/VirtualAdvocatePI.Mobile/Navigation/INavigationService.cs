namespace VirtualAdvocatePI.Mobile.Navigation;

public interface INavigationService
{
    Task GoToAsync(string route);

    Task GoToAsync(string route, IDictionary<string, object> parameters);

    Task GoBackAsync();

    Task GoToRootAsync(string route);

    Task GoToRootAsync(string route, IDictionary<string, object> parameters);
}
