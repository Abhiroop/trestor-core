//
// SplitPage.xaml.cpp
// Implementation of the SplitPage class
//

#include "pch.h"
#include "SplitPage.xaml.h"

using namespace TNET_UI;

using namespace Platform;
using namespace Platform::Collections;
using namespace concurrency;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::Graphics::Display;
using namespace Windows::UI::ViewManagement;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Interop;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;

// The Split Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234234

SplitPage::SplitPage()
{
	InitializeComponent();
	SetValue(_defaultViewModelProperty, ref new Map<String^,Object^>(std::less<String^>()));
	auto navigationHelper = ref new Common::NavigationHelper(this,
		ref new Common::RelayCommand(
			[this](Object^) -> bool
			{
				return CanGoBack();
			},
			[this](Object^) -> void
			{
				GoBack();
			}
		)
	);
	SetValue(_navigationHelperProperty, navigationHelper);
	navigationHelper->LoadState += ref new Common::LoadStateEventHandler(this, &SplitPage::LoadState);
	navigationHelper->SaveState += ref new Common::SaveStateEventHandler(this, &SplitPage::SaveState);

	itemListView->SelectionChanged += ref new SelectionChangedEventHandler(this, &SplitPage::ItemListView_SelectionChanged);
	Window::Current->SizeChanged += ref new WindowSizeChangedEventHandler (this, &SplitPage::Window_SizeChanged);
	InvalidateVisualState();
}

DependencyProperty^ SplitPage::_defaultViewModelProperty =
	DependencyProperty::Register("DefaultViewModel",
		TypeName(IObservableMap<String^,Object^>::typeid), TypeName(SplitPage::typeid), nullptr);


/// <summary>
/// used as a trivial view model.
/// </summary>
IObservableMap<String^, Object^>^ SplitPage::DefaultViewModel::get()
{
	return safe_cast<IObservableMap<String^, Object^>^>(GetValue(_defaultViewModelProperty));
}

DependencyProperty^ SplitPage::_navigationHelperProperty =
	DependencyProperty::Register("NavigationHelper",
		TypeName(Common::NavigationHelper::typeid), TypeName(SplitPage::typeid), nullptr);

/// <summary>
/// Gets an implementation of <see cref="NavigationHelper"/> designed to be
/// used as a trivial view model.
/// </summary>
Common::NavigationHelper^ SplitPage::NavigationHelper::get()
{
	//	return _navigationHelper;
	return safe_cast<Common::NavigationHelper^>(GetValue(_navigationHelperProperty));
}

bool SplitPage::CanGoBack()
{
	if (UsingLogicalPageNavigation() && itemListView->SelectedItem != nullptr)
	{
		return true;
	}
	else
	{
		return NavigationHelper->CanGoBack();
	}
}
void SplitPage::GoBack()
{
	if (UsingLogicalPageNavigation() && itemListView->SelectedItem != nullptr)
	{
		// When logical page navigation is in effect and there's a selected item that
		// item's details are currently displayed.  Clearing the selection will return to
		// the item list.  From the user's point of view this is a logical backward
		// navigation.
		itemListView->SelectedItem = nullptr;
	}
	else
	{
		NavigationHelper->GoBack();
	}
}

/// <summary>
/// Invoked with the Window changes size
/// </summary>
/// <param name="sender">The current Window</param>
/// <param name="e">Event data that describes the new size of the Window</param>
void SplitPage::Window_SizeChanged(Platform::Object^ sender, Windows::UI::Core::WindowSizeChangedEventArgs^ e)
{
	InvalidateVisualState();
}

/// <summary>
/// Invoked when an item within the list is selected.
/// </summary>
/// <param name="sender">The GridView displaying the selected item.</param>
/// <param name="e">Event data that describes how the selection was changed.</param>
void SplitPage::ItemListView_SelectionChanged(Platform::Object^ sender, Windows::UI::Xaml::Controls::SelectionChangedEventArgs^ e)
{
	if (UsingLogicalPageNavigation())
	{
		InvalidateVisualState();
	}
}

/// <summary>
/// Invoked to determine whether the page should act as one logical page or two.
/// </summary>
/// <returns>True if the window should show act as one logical page, false
/// otherwise.</returns>
bool SplitPage::UsingLogicalPageNavigation()
{
	return Windows::UI::Xaml::Window::Current->Bounds.Width < 768;
}

void SplitPage::InvalidateVisualState()
{
	auto visualState = DetermineVisualState();
	Windows::UI::Xaml::VisualStateManager::GoToState(this, visualState, false);
	NavigationHelper->GoBackCommand->RaiseCanExecuteChanged();
}

/// <summary>
/// Invoked to determine the name of the visual state that corresponds to an application
/// view state.
/// </summary>
/// <returns>The name of the desired visual state.  This is the same as the name of the
/// view state except when there is a selected item in portrait and snapped views where
/// this additional logical page is represented by adding a suffix of _Detail.</returns>
Platform::String^ SplitPage::DetermineVisualState()
{
	if (!UsingLogicalPageNavigation())
		return "PrimaryView";

	// Update the back button's enabled state when the view state changes
	auto logicalPageBack = UsingLogicalPageNavigation() && itemListView->SelectedItem != nullptr;

	return logicalPageBack ? "SinglePane_Detail" : "SinglePane";
}

/// <summary>
/// Populates the page with content passed during navigation.  Any saved state is also
/// provided when recreating a page from a prior session.
/// </summary>
/// <param name="sender">
/// The source of the event; typically <see cref="NavigationHelper"/>
/// </param>
/// <param name="e">Event data that provides both the navigation parameter passed to
/// <see cref="Frame::Navigate(Type, Object)"/> when this page was initially requested and
/// a dictionary of state preserved by this page during an earlier
/// session.  The state will be null the first time a page is visited.</param>
void SplitPage::LoadState(Object^ sender, Common::LoadStateEventArgs^ e)
{
	(void) sender;	// Unused parameter

	// TODO: Create an appropriate data model for your problem domain to replace the sample data
	Data::SampleDataSource::GetGroup(safe_cast<String^>(e->NavigationParameter))
	.then([this, e](Data::SampleDataGroup^ group)
	{
		DefaultViewModel->Insert("Group", group);
		DefaultViewModel->Insert("Items", group->Items);
		if (e->PageState == nullptr)
		{
			this->itemListView->SelectedItem = nullptr;
			if (!UsingLogicalPageNavigation() && itemsViewSource->View != nullptr)
			{
				itemsViewSource->View->MoveCurrentToFirst();
			}
		}
		else
		{
			if (e->PageState->HasKey("SelectedItem") && itemsViewSource->View != nullptr)
			{
				Data::SampleDataSource::GetItem(safe_cast<String^>(e->PageState->Lookup("SelectedItem")))
					.then([this](Data::SampleDataItem^ selectedItem)
				{
					itemsViewSource->View->MoveCurrentTo(selectedItem);
				}, task_continuation_context::use_current());
			}
		}
	}, task_continuation_context::use_current());
}

/// <summary>
/// Preserves state associated with this page in case the application is suspended or the
/// page is discarded from the navigation cache.  Values must conform to the serialization
/// requirements of <see cref="SuspensionManager::SessionState"/>.
/// </summary>
/// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
/// <param name="e">Event data that provides an empty dictionary to be populated with
/// serializable state.</param>
void SplitPage::SaveState(Object^ sender, Common::SaveStateEventArgs^ e)
{
	auto selectedItem = safe_cast<Data::SampleDataItem^>(itemListView->SelectedItem);
	if (selectedItem != nullptr)
	{
		e->PageState->Insert("SelectedItem", selectedItem->UniqueId);
	}
}

#pragma region Navigation support

/// The methods provided in this section are simply used to allow
/// NavigationHelper to respond to the page's navigation methods.
/// 
/// Page specific logic should be placed in event handlers for the  
/// <see cref="NavigationHelper::LoadState"/>
/// and <see cref="NavigationHelper::SaveState"/>.
/// The navigation parameter is available in the LoadState method 
/// in addition to page state preserved during an earlier session.

void SplitPage::OnNavigatedTo(NavigationEventArgs^ e)
{
	NavigationHelper->OnNavigatedTo(e);
}

void SplitPage::OnNavigatedFrom(NavigationEventArgs^ e)
{
	NavigationHelper->OnNavigatedFrom(e);
}

#pragma endregion