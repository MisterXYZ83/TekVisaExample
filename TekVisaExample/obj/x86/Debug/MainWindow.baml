   M S B A M L   `   `   `  ����I  ETekVisaExample, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:ControllerSirenaControllerSirena  5   	   /clr-namespace:TekVisaExampleTekVisaExample  5   	       TekVisaExample.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   3controlsclr-namespace:ControllerSirena     5   	   3
phonometerclr-namespace:TekVisaExample     5   	     � Title$  Misuratore Buzzer��5      $	��472��6   $	��837��6,   .��5	      � $��
10,10,10,5q�6   .+�5
   
   "   ControllerSirena.ToneManager  -   5          Name$ toneManager��5
      $��
0,50,330,0q�64   $��Stretch=�6H   $	��150��6f   $	��Top=�6s   5      �� 5   
   -   5         ��Name$ startSweepButton��5      $��Avvia Misura��6+   $
��Left=�6B   $��677,259,0,0q�6]   $	��Top=�6r   $	��115��6�   $��30��6�   �� 5   
   -   5         ��Name$ instrumentsCombo��5      $
��Left=�6-   $��
80,205,0,0q�6H   $	��Top=�6\   $	��238��6t   �� 5   
   -   5        $ 
scanButton��5      $��Scan USB��6%   $
��Left=�68   $��	0,205,0,0q�6S   $	��Top=�6f   $��75��6~   $��22��6�   6�   �� 5   
   -   5         ��Name$ label1��5      $��COM Fonometro:��6    $
��Left=�69   $��	10,10,0,0q�6T   $	��Top=�6g    J� RenderTransformOrigin$ -0.658,-1.115&�6   �� 5   
   -   5        $ comPortCombo��5      $
��Left=�6)   $��
116,10,0,0q�6D   $	��Top=�6X   $	��103��6p   �� 5   
   -   5        $ rescanButton��5      $��
Rescan COM��6'   $
��Left=�6<   $��
398,13,0,0q�6W   $	��Top=�6k   $��75��6�   $ 	0.387,1.6&�6�   6�   �� 5   
   -   5        $ testComButton��5      $��Test Fonometro��6(   $
��Left=�6A   $��
301,13,0,0q�6\   $	��Top=�6p   $��92��6�   6�   � 5   
   $��
292,0,0,10q�6   $��Stretch=�6#   $��Bottom=�6A   $	��500��6\   .+�5      �� -	   5        $ label��5      $��Nome Misura:��6#   $
��Left=�6:   $��0,0,0,0q�6U   $	��Top=�6f   $	��101��6~   �� 5      -
   5        $ label2��5      $��Data:��6$   $
��Left=�64   $��0,30,0,0q�6O   $	��Top=�6a   �� 5      -   5        $ label3��5      $��Buzzer:��6$   $
��Left=�66   $��0,60,0,0q�6Q   $	��Top=�6c   �� 5      -   5        $ label4��5      $��Descrizione:��6$   $
��Left=�6;   $��0,90,0,0q�6V   $	��Top=�6h   �� 5      -   5         ��Name$ nameText��5      $��23��6)   $��
116,0,10,0q�65    �� TextWrapping$
 Wrap=�6I   $	��Top=�6]   $��Stretch=�6u   (  "System.Windows.Controls.DatePicker  5      -   5          Name$ sessionDate��5      $
��Left=�6.   $��
116,30,0,0q�6I   $	��Top=�6]   �� 5      -   5        $ 
buzzerText��5      $��Stretch=�6*   $��23��6H   $��116,60,10,0q�6T   $
 Wrap=�6i   $	��Top=�6}   �� 5      -   5        $ descriptionText��5      $��Stretch=�6/   $��23��6M   $��116,90,10,0q�6Y   $
 Wrap=�6n   $	��Top=�6�   5      �� 5   
   -   5        $ freqStepCombo��5      $
��Left=�6*   $��145,346,0,0q�6E   $	��Top=�6Z   $��86��6r   �� 5    
   -   5        $ label6��5       $��Freq. Step:��6    $
��Left=�66   $��	0,346,0,0q�6Q   $	��Top=�6d   �� 5!   
   -   5        $ label7��5!      $
��Vpp:��6    $
��Left=�6/   $��	0,315,0,0q�6J   $	��Top=�6]   �� 5"   
   -   5        $ amplitudeCombo��5"      $
��Left=�6+   $��145,315,0,0q�6F   $	��Top=�6[   $��86��6s   �� 5#   
   -   5        $ phonoLagCombo��5#      $
��Left=�6*   $��145,285,0,0q�6E   $	��Top=�6Z   $��86��6r   �� 5$   
   -   5        $ label8��5$      $��Ritardo acq. fonometro:��6    $
��Left=�6B   $��	0,285,0,0q�6]   $	��Top=�6p   $ 0.842,-0.115&�6�   $	��140��6�   &    TekVisaExample.PhonometerDisplay  5&   
   -   5        	  Name$	 phonoDisplay��5&   '   $��Right=�6=   $	��Top=�6Y   $��	0,10,10,0q�6q   ���	#FF9797976�   �� 5'   
   -   5        $ 
plotButton��5'      $��Finestra Analisi��6%   $
��Left=�6@   $��677,234,0,0q�6[   $	��Top=�6p   $	��115��6�   6�   �� 5(   
   -   5        $ label9��5(      $��IP Oscilloscopio:��6    $
��Left=�6<   $��	0,242,0,0q�6W   $	��Top=�6j   �� 5)   
   -   5        $ oscilloscopeCheckButton��5)      $��Check��62   $
��Left=�6B   $��323,246,0,0q�6]   $	��Top=�6r   $��75��6�   $��22��6�   6�   �� 5*   
   -   5        $ label6_Copy��5*      $��Impedenza di misura:��6%   $
��Left=�6D   $��	0,377,0,0q�6_   $	��Top=�6r   �� 5+   
   -   5        $ ipOscilloscopeText��5+      $
��Left=�6.   $��23��6I   $��101,245,0,0q�6U   $
 Wrap=�6j   $	��Top=�6~   $	��217��6�   �� 5,   
   -   5        $ label9_Copy��5,      $��Canale Oscilloscopio:��6%   $
��Left=�6E   $��472,265,0,0q�6`   $	��Top=�6u   �� 5-   
   -   5        $ 
probeCombo��5-      $
��Left=�6'   $��599,267,0,0q�6B   $	��Top=�6W   $��56��6o   �� 5.   
   -   5        $ impedanceMeasureText��5.      $
��Left=�60   $��23��6K   $��145,377,0,0q�6W   $
 Wrap=�6l   $��50��6�   $	��Top=�6�   $��86��6�   �� 5/   
   -    5        
 ��Name$
 phonoEnableCheck��5/      $��Cattura da Fonometro��6-   $
��Left=�6L   $��472,225,0,0q�6g   $	��Top=�6|    d� 	IsChecked$
 TrueY�6�   �� 50   
   -!   5        $
 oscilloscopeEnableCheck��50      $��Cattura da oscilloscopio��64   $
��Left=�6W   $��472,245,0,0q�6r   $	��Top=�6�   $
 TrueY�6�   52      53      