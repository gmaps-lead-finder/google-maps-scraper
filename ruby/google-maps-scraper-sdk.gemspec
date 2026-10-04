# frozen_string_literal: true

require_relative "lib/gmaps_scraper/version"

Gem::Specification.new do |spec|
  spec.name = "google-maps-scraper-sdk"
  spec.version = GmapsScraper::VERSION
  spec.authors = ["GMaps Lead Finder"]
  spec.email = ["support@gmapsleadfinder.com"]

  spec.summary = "Google Maps Scraper / Extractor / Lead Scraper SDK for GMaps Lead Finder"
  spec.description =
    "Official Ruby client for the GMaps Lead Finder Agent HTTP API — scrape Google Maps " \
    "places (name, phone, website, emails) via a hosted cloud pipeline."
  spec.homepage = "https://gmapsleadfinder.com"
  spec.license = "MIT"
  spec.required_ruby_version = ">= 3.1.0"

  spec.metadata["homepage_uri"] = spec.homepage
  spec.metadata["source_code_uri"] =
    "https://github.com/gmaps-lead-finder/google-maps-scraper/tree/main/ruby"
  spec.metadata["changelog_uri"] =
    "https://github.com/gmaps-lead-finder/google-maps-scraper/blob/main/CHANGELOG.md"
  spec.metadata["documentation_uri"] = "https://gmapsleadfinder.com/docs/api"
  spec.metadata["bug_tracker_uri"] =
    "https://github.com/gmaps-lead-finder/google-maps-scraper/issues"
  spec.metadata["rubygems_mfa_required"] = "true"

  spec.files = Dir.chdir(__dir__) do
    Dir[
      "lib/**/*",
      "exe/*",
      "examples/**/*",
      "README.md",
      "LICENSE",
      "google-maps-scraper-sdk.gemspec"
    ].select { |f| File.file?(f) }
  end
  spec.bindir = "exe"
  spec.executables = ["gmaps-scraper"]
  spec.require_paths = ["lib"]
end
