$imageTag = $args[0]
$env = $args[0]
write-host "Building image for tag $($imageTag)"

if ($env -eq "stag") {
    docker build -f docker\Dockerfile.backend -t asia.gcr.io/staging-kps-ecommerce/kps-integration:$imageTag .
    docker push asia.gcr.io/staging-kps-ecommerce/kps-integration:$imageTag
}
else {
    docker build -f docker\Dockerfile.backend -t asia.gcr.io/prod-kps-ecommerce/kps-integration:$imageTag .
    docker push asia.gcr.io/prod-kps-ecommerce/kps-integration:$imageTag
}