package main

import (
	"fmt"
	"os"

	gmaps "github.com/gmaps-lead-finder/google-maps-scraper/go"
)

const keyword = "dentists in Austin TX"

func main() {
	client, err := gmaps.NewClient(nil)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(1)
	}
	me, err := client.Me()
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(1)
	}
	fmt.Printf("plan=%s creditsRemaining=%v\n", me.Plan, me.CreditsRemaining)

	rows, err := client.Scrape(keyword, nil)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(1)
	}
	fmt.Printf("scraped %d places for %q\n", len(rows), keyword)
	if len(rows) > 0 {
		r := rows[0]
		fmt.Println(r["Name"], r["Phone"], r["Website"], r["Emails"])
	}
}
