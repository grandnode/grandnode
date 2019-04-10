param(
    [string]$branch = 'develop'
)

git pull https://github.com/grandnode/grandnode.git $branch

git push origin $branch