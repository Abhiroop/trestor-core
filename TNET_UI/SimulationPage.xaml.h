//
// SimulationPage.xaml.h
// Declaration of the SimulationPage class
//

#pragma once

#include "SimulationPage.g.h"

namespace TNET_UI
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	[Windows::Foundation::Metadata::WebHostHidden]
	public ref class SimulationPage sealed
	{
	public:
		SimulationPage();
	private:
		void Button_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
		void button_StartSimulation_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
		void PrintMessage(Platform::String^ msg);
		void TestTX();
		void button_TransactionTest_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
		void AritaTest_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
	};
}
