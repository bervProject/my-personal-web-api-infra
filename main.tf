terraform {
  required_providers {
    heroku = {
      source = "heroku/heroku"
      version = "4.8.0"
    }
  }
}

provider "heroku" {
}

resource "heroku_app" "default" {
  name   = "berviantoleo"
  region = "us"
  stack = "container"
}

resource "heroku_addon" "database" {
  app  = heroku_app.default.name
  plan = "heroku-postgresql:hobby-dev"
}

resource "heroku_build" "build_app" {
  app = heroku_app.default.name

  source {
    # This app uses a community buildpack, set it in `buildpacks` above.
    url     = "https://github.com/bervProject/my-telegram-bot/archive/refs/tags/v1.0.0.tar.gz"
    version = "v1.0.0"
  }
}
