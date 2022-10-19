# Obtain D365FOLabelUtilities

## LabelEditor
Okay, this is probably the first C# project that I have actually ended, so bear with me, I am not a particular good C# developer. Oh, I'm not used to use Git, so any advice in how it is best used, let me know. 

I do like constructive feedback, and suggestion for changes as long as you also state _why_ the change makes sense.

### App.setting
It will contain a couple of keys used to connect to Azure translator, i.e. a translater key, endpoint (which I think is rather static), and the region in which you have created the translator

### How to use it
The application needs to run as an administrator in order to be allowed to change the label text files. It will still run, but it might fail in the attempt to save the text files.
When you load a file, it will ask for the xml label wrapper file. It doesn't really do anything with them except decipher what languages are in scope. It will then load the associated text files for the different languages.

For translation, right click the cell that contain the text you want to have translated. It will populate the other blank language cells with the translated text. So, if there is already a text in the cell, it will leave it alone.