import sys, argparse, json
import cloudscraper

# Read command line arguments.

parser = argparse.ArgumentParser()

parser.add_argument('url', nargs = 1)
parser.add_argument('--proxy', nargs = '?')
parser.add_argument('--user-agent', nargs = '?')

args = parser.parse_args()

# Get tokens.

browser = None
proxies = None

if args.user_agent is not None:
   browser = {
        "custom": args.user_agent
    }

if args.proxy is not None:
    proxies = {
        "http": args.proxy,
        "https": args.proxy
    }

tokens = cloudscraper.get_tokens(args.url[0], proxies = proxies, browser = browser)

# Return the tokens as a JSON object.

print(json.dumps(tokens))