# Introduction
When we commit a change to the database then some unexpect error happened we alway want those changed data to roll-back to its original sate.
Why do we want it to be that way? Because we want our data to stay consistency. In this short article I will talk about database transaction
and how I handle it; and to be clear I will not say this is the most optimal solution but I hope it help.
