   M S B A M L   `   `   `  ����I  ETekVisaExample, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:TekVisaExampleTekVisaExample  5   	       TekVisaExample.PlotWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   .localclr-namespace:TekVisaExample     5   	   R NOxyPlot.Wpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=75e952ba404cdbb0 oxyhttp://oxyplot.org/wpf  5   	     � Title$  Analisi Dati��5	   	   $
��1128��6   $	��821��6,   .��5      �  � ColumnDefinitions 5   
   �� 5      .+�5   
   �� -   5         ��Name$ seriesCombo��5      $
��Left=�6(   $��139,527,0,30q�6C   $��Bottom=�6Y   $	��217��6t   �� 5   
   -   5         ��Name$ removeSeries��5      $��Rimuovi Selezionata��6'   $
��Left=�6E   $��498,527,0,30q�6`   $��Bottom=�6v   $	��130��6�   $��22��6�   6�   �� 5   
   -   5        $ 	exportXLS��5      $��Esporta Dati XLS��6$   $
��Left=�6?   $��633,527,0,30q�6Z   $��Bottom=�6p   $	��130��6�   $��22��6�   6�   �� 5   
   -   5         ��Name$ 
label_Copy��5      $��Registrazioni:��6$   $
��Left=�6=   $��20,525,0,30q�6X   $��Bottom=�6m   $	��101��6�   � 5   
   $��Stretch=�6   $��Stretch=�6-   $��
10,10,0,90q�6I    5      �� 5      $��* �6#   �� 5      $��* �6#   .+�5        OxyPlot.Wpf.PlotView  -   5        $��	audioPlot��5      $��0��6,   $��5,5,5,5q�6<   6O     5      -   5        $��currentPlot��5      $��1��6.   $��5,5,5,5q�6>   6Q   5      5!      5"      