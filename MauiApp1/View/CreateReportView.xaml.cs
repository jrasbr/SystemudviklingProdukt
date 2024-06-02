using MauiApp1.ViewModel;

namespace MauiApp1.View;

public partial class CreateReportView : ContentPage
{

	public CreateReportView(CreateReportViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}