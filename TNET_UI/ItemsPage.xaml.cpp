//
// ItemsPage.xaml.cpp
// Implementation of the ItemsPage class
//

#include "pch.h"
#include "ItemsPage.xaml.h"
#include "SplitPage.xaml.h"

using namespace TNET_UI;

using namespace Platform;
using namespace Platform::Collections;
using namespace concurrency;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Interop;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

ItemsPage::ItemsPage()
{
	InitializeComponent();
	SetValue(_defaultViewModelProperty, ref new Map<String^,Object^>(std::less<String^>()));
	auto navigationHelper = ref new Common::NavigationHelper(this);
	SetValue(_navigationHelperProperty, navigationHelper);
	navigationHelper->LoadState += ref new Common::LoadStateEventHandler(this, &ItemsPage::LoadState);
}

DependencyProperty^ ItemsPage::_defaultViewModelProperty =
	DependencyProperty::Register("DefaultViewModel",
		TypeName(IObservableMap<String^,Object^>::typeid), TypeName(ItemsPage::typeid), nullptr);


/// <summary>
/// used as a trivial view model.
/// </summary>
IObservableMap<String^, Object^>^ ItemsPage::DefaultViewModel::get()
{
	return safe_cast<IObservableMap<String^, Object^>^>(GetValue(_defaultViewModelProperty));
}

DependencyProperty^ ItemsPage::_navigationHelperProperty =
	DependencyProperty::Register("NavigationHelper",
		TypeName(Common::NavigationHelper::typeid), TypeName(ItemsPage::typeid), nullptr);

/// <summary>
/// Gets an implementation of <see cref="NavigationHelper"/> designed to be
/// used as a trivial view model.
/// </summary>
Common::NavigationHelper^ ItemsPage::NavigationHelper::get()
{
	return safe_cast<Common::NavigationHelper^>(GetValue(_navigationHelperProperty));
}

/// <summary>
/// Populates the page with content passed during navigation.  Any saved state is also
/// provided when recreating a page from a prior session.
/// </summary>
/// <param name="sender">
/// The source of the event; typically <see cref="NavigationHelper"/>
/// </param>
/// <see cref="Frame::Navigate(Type, Object)"/> when this page was initially requested and
/// a dictionary of state preserved by this page during an earlier
/// session.  The state will be null the first time a page is visited.</param>
void ItemsPage::LoadState(Object^ sender, Common::LoadStateEventArgs^ e)
{
	(void) e;		// Unused parameter
	(void) sender;	// Unused parameter

	// TODO: Create an appropriate data model for your problem domain to replace the sample data
	Data::SampleDataSource::GetGroups()
	.then([this](IIterable<Data::SampleDataGroup^>^ sampleDataGroups)
	{
		DefaultViewModel->Insert("Items", sampleDataGroups);
	}, task_continuation_context::use_current());
}

/// <summary>
/// Invoked when an item is clicked.
/// </summary>
/// <param name="sender">The GridView (or ListView when the application is snapped)
/// displaying the item clicked.</param>
/// <param name="e">Event data that describes the item clicked.</param>
void ItemsPage::ItemView_ItemClick(Object^ sender, ItemClickEventArgs^ e)
{
	(void) sender;	// Unused parameter

	// Navigate to the appropriate destination page, configuring the new page
	// by passing required information as a navigation parameter
	auto groupId = safe_cast<Data::SampleDataGroup^>(e->ClickedItem)->UniqueId;
	Frame->Navigate(TypeName(SplitPage::typeid), groupId);
}

#pragma region NavigationHelper registration

/// The methods provided in this section are simply used to allow
/// NavigationHelper to respond to the page's navigation methods.
/// 
/// Page specific logic should be placed in event handlers for the  
/// <see cref="NavigationHelper::LoadState"/>
/// and <see cref="NavigationHelper::SaveState"/>.
/// The navigation parameter is available in the LoadState method 
/// in addition to page state preserved during an earlier session.

void ItemsPage::OnNavigatedTo(NavigationEventArgs^ e)
{
	NavigationHelper->OnNavigatedTo(e);
}

void ItemsPage::OnNavigatedFrom(NavigationEventArgs^ e)
{
	NavigationHelper->OnNavigatedFrom(e);
}

#pragma endregion
