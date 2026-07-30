using VirtualAdvocatePI.Mobile.Navigation;
using VirtualAdvocatePI.Mobile.Pages;

namespace VirtualAdvocatePI.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(Routes.NewClaimWorkspace, typeof(NewClaimWorkspacePage));
		Routing.RegisterRoute(Routes.ClaimWorkspaceDetail, typeof(ClaimWorkspaceDetailPage));
		Routing.RegisterRoute(Routes.ConditionList, typeof(ConditionListPage));
		Routing.RegisterRoute(Routes.GarpMQuestionEngine, typeof(GarpMQuestionEnginePage));
	}
}
