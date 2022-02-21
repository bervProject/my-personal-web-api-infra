terraform {
  required_providers {
    heroku = {
      source  = "heroku/heroku"
      version = "5.0.0"
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
    url     = "https://github.com/bervProject/MyPersonalWebAPI/archive/refs/tags/v0.2.0.tar.gz"
    version = "0.2.0"
  }
}
