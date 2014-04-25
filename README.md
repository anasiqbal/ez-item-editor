Game Data Editor 
==============

The Game Data Editor is a Unity plugin that allows you to define &
create a data set for your game all within an inspector window. This
is for game settings that need to be tweaked during development or any
settings you want to store without hand editing files. We provide an
API to retrieve the settings during runtime of the game.

There are two tabs to the plugin, one to Define the dataset and one to
Create the dataset.  This allows you to specify the data types and
variable names for the data on the Define tab.  It supports all the
basic data types including lists.

On the Create tab you use the schemas that you made on the Define tab
to create instances of the data.  Under the covers, the dataset is
stored in a json file which is then consumed during the runtime of
your game using our API (link here).

## Some Sample Code of consuming an Item ## 

