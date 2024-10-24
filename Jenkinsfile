pipeline {
    agent any

    stages {
        stage('Clone Repository') {
            steps {
                git branch: 'main', url: 'https://github.com/suleymanovdev/prometeyapi', credentialsId: 'jenkins'
            }
        }
        stage('Docker Compose Down') {
            steps {
                script {
                    sh 'docker-compose down'
                }
            }
        }
        stage('Build and Start Containers') {
            steps {
                script {
                    sh 'docker-compose up --build -d'
                }
            }
        }
    }

    post {
        always {
            echo 'Prometey API Successfully deployed.'
        }
    }
}
