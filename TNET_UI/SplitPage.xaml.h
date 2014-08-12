//
// GroupDetailPage.xaml.h
// Declaration of the GroupDetailPage class
//

#pragma once

#include "SplitPage.g.h"

namespace TNET_UI
{
	/// <summary>
	/// A page that displays an overview of a single group, including a preview of the items
	/// within the group.
	/// </summary>
	[Windows::UI::Xaml::Data::Bindable]
	public ref class SplitPage sealed
	{
	public:
		SplitPage();
		/// <summary>
		/// This can be changed to a strongly typed view model.
		/// </summary>
		property Windows::Foundation::Collections::IObservableMap<Platform::String^, Platform::Object^>^ DefaultViewModel
		{
			Windows::Foundation::Collections::IObservableMap<Platform::String^, Platform::Object^>^  get();
		}

		/// <summary>
		/// NavigationHelper is used on each page to aid in navigation and 
		/// process lifetime management
		/// </summary>
		/// <summary>
		/// NavigationHelper is used on each page to aid in navigation and 
		/// process lifetime management
		/// </summary>
		property Common::NavigationHelper^ NavigationHelper
		{
			Common::NavigationHelper^ get();
		}

	protected:
		virtual void OnNavigatedTo(Windows::UI::Xaml::Navigation::NavigationEventArgs^ e) override;
		virtual void OnNavigatedFrom(Windows::UI::Xaml::Navigation::NavigationEventArgs^ e) override;

	private:
		void LoadState(Platform::Object^ sender, Common::LoadStateEventArgs^ e);
		void SaveState(Object^ sender, Common::SaveStateEventArgs^ e);
		void ItemView_ItemClick(Platform::Object^ sender, Windows::UI::Xaml::Controls::ItemClickEventArgs^ e);

        bool CanGoBack();
        void GoBack();

#pragma region Logical page navigation

        // The split page isdesigned so that when the Window does have enough space to show
        // both the list and the dteails, only one pane will be shown at at time.
        //
        // This is all implemented with a single physical page that can represent two logical
        // pages.  The code below achieves this goal without making the user aware of the
        // distinction.

        void Window_SizeChanged(Platform::Object^ sender, Windows::UI::Core::WindowSizeChangedEventArgs^ e);
        void ItemListView_SelectionChanged(Platform::Object^ sender, Windows::UI::Xaml::Controls::SelectionChangedEventArgs^ e);
        bool UsingLogicalPageNavigation();
        void InvalidateVisualState();
        Platform::String^ DetermineVisualState();

#pragma endregion

		static Windows::UI::Xaml::DependencyProperty^ _navigationHelperProperty;
		static Windows::UI::Xaml::DependencyProperty^ _defaultViewModelProperty;
		static const int MinimumWidthForSupportingTwoPanes = 768;

	};
}
