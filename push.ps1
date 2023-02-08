$imageTag=$args[0]
write-host "Publishing image for tag $($imageTag)"

docker image tag asia.gcr.io/staging-kps-ecommerce/kps-integration:$imageTag asia.gcr.io/prod-kps-ecommerce/kps-integration:$imageTag

docker push asia.gcr.io/prod-kps-ecommerce/kps-integration:$imageTag