   M S B A M L   `   `   `  ����  CSSTClientStatisticModule7    1CSSTClientStatisticModule.CSSTClientStatisticView   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      .��5      �   � RowDefinitions  5   
   �� 5	      �� 5
      �� 5      �� 5      �� 5      �� 5       � ColumnDefinitions 5   
   �� 5      �� 5      �� 5      .+�5   
   �� $��Feature��6   $��0��6"   $��Center=�6/   �� 5   
   $��Subject Measurement��6   $��0��6.   $��1��6;   $��Center=�6K   �� 5   
   $��Healthy Measurement��6   $��0��6.   $��2��6;   $��Center=�6K   �� 5   
   $��Cycles��6   $��1��6!   $��0��6.   $��Center=�6>   �� 5   
   $��1��60   $��1��6=   $��Center=�6M   ��6   �� *cycleNumber+6M   �� 5   
   $��1��67   $��2��6D   $��Center=�6T   ��6   �� *healthyCycleNumber+6T   �� 5   
   $��Standup Duration��6   $��2��6+   $��0��68   $��Center=�6H   �� 5   
   $��2��62   $��1��6?   $��Center=�6O   ��6   �� *upTimeMeanStd+6O   �� 5   
   $��2��62   $��2��6?   $��Center=�6O   ��6   �� *healthyUpTime+6O   �� 5   
   $��Sit Down Duration��6   $��3��6,   $��0��69   $��Center=�6I   �� 5   
   $��3��64   $��1��6A   $��Center=�6Q   ��6   �� *downTimeMeanStd+6Q   �� 5    
   $��3��64   $��2��6A   $��Center=�6Q   ��6   �� *healthyDownTime+6Q   �� 5!   
   $��Body Swing Magnitude��6   $��4��6/   $��0��6<   $��Center=�6L   �� 5"   
   $��4��65   $��1��6B   $��Center=�6R   ��6   �� *bodySwingMeanStd+6R   �� 5#   
   $��4��65   $��2��6B   $��Center=�6R   ��6   �� *healthyBodySwing+6R   �� 5$   
   $��Arm Swing Magnitude��6   $��5��6.   $��0��6;   $��Center=�6K   �� 5%   
   $��5��64   $��1��6A   $��Center=�6Q   ��6   �� *armSwingMeanStd+6Q   �� 5&   
   $��5��64   $��2��6A   $��Center=�6Q   ��6   �� *healthyArmSwing+6Q   5'      5(      