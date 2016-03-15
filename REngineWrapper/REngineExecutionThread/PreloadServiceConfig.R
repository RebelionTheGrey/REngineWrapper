rm(list = ls(all = TRUE));
devNull <- sapply(libraries, library, character.only = TRUE);

libraries <- c("wmtsa", "fGarch", "fArma", "broman", "parallel", "forecast", "Rsolnp");
sapply(libraries, library, character.only = TRUE);

internalCluster <- max(1, detectCores() - 1);

clusterExport(internalCluster, libraries);
clusterEvalQ(internalCluster, sapply(libraries, library, character.only = TRUE));
