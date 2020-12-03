gcloud projects list | grep -v '^PROJECT_ID' | awk '{print $1}' | xargs -P 4 -l ./getpeerings.sh
dotnet run .
