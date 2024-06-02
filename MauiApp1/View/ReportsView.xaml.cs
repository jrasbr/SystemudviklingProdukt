using MauiApp1.ViewModel;

namespace MauiApp1.View;

public partial class ReportsView : ContentPage
{
	public ReportsView(ReportsViewModel reportsViewModel)
	{
		InitializeComponent();
		BindingContext = reportsViewModel;
	}

	override protected void OnAppearing()
	{
        base.OnAppearing();
        ((ReportsViewModel)BindingContext).LoadData();
    }
	
	//public ReportsView()
	//{
	//	InitializeComponent();
 //      // BindingContext = reportsViewModel;
 //   }
}