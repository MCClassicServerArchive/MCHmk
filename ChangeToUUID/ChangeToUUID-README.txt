ChangeToUUID README
===================

This program converts your server's data for MCHmk r17.
The program must be run before updating your server to
r17, or else many errors will occur.

This README file contains important information about
this program and should be read carefully to minimize
the likelihood of data loss.

Backing Up Data
---------------

Before using this program, please make a backup of your
server's folder and your database. For SQLite users,
simply make a copy of your MCHmk.db file. For MySQL,
it is best to use the mysqldump tool to back up
your database.

An example command for mysqldump may look like:

mysqldump -u [user] -p[password] [database] > file.txt

Store file.txt in a location outside your server
directory.

Running the Program
-------------------

The program must be extracted to the base of the
server folder; in other words, the same folder as
MCHmkCLI.exe.

Open a command line and browse to that folder.
Then run the program. If you are using Mono, use the
following command:

mono ChangeToUUID.exe

Follow the instructions on the screen to start the
conversion process. In particular, make sure you have
a working Internet connection and that MySQL, if it 
is being used, is currently running.

This process can take at least an hour, depending on
the number of players recorded in your database.

After Converting
----------------

After the conversion finishes, you may update to
MCHmk r17. You should delete ChangeToUUID.exe as well.

Please note that all admin passwords will become
invalid as a result of the process. Any admins will
have to use /setpass again.

Need More Help?
---------------

For more information about what this program actually
does, please visit the MCHmk wiki at:

https://bitbucket.org/Jjp137/mchmk/wiki/

If problems occur, please create an issue at:

https://bitbucket.org/Jjp137/mchmk/issues/
