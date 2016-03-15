rm(list = ls(all = TRUE));
devNull <- sapply(libraries, library, character.only = TRUE);

libraries <- c("parallel");
sapply(libraries, library, character.only = TRUE);