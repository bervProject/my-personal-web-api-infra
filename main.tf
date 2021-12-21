terraform {
  required_providers {
    heroku = {
      source  = "heroku/heroku"
      version = "4.8.0"
    }
  }

  cloud {
    organization = "bervproject"
    workspaces {
      name = "my-personal-web-api-infra"
    }
  }
}

provider "heroku" {
}

resource "heroku_app" "default" {
  name   = "berviantoleo"
  region = "us"
  stack  = "container"
}

resource "heroku_addon" "database" {
  app  = heroku_app.default.name
  plan = "heroku-postgresql:hobby-dev"
}

resource "heroku_build" "build_app" {
  app = heroku_app.default.name

  source {
    url     = "https://github.com/bervProject/PersonalWebApi/archive/refs/tags/v0.1.1.tar.gz"
    version = "0.1.1"
  }
}
