import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";

const features = [
  {
    title: "Real, vetted testers",
    description:
      "Every tester is a real person, not a bot or a rubber-stamp reviewer. Get feedback you can actually act on.",
    icon: "👤",
  },
  {
    title: "Structured feedback",
    description:
      "Tasks, bug reports, and evidence come back in a consistent format — no more sifting through scattered notes.",
    icon: "📋",
  },
  {
    title: "Fast turnaround",
    description:
      "Launch a test and start seeing results in hours, not weeks. Iterate quickly on real user input.",
    icon: "⚡",
  },
  {
    title: "Built for research",
    description:
      "Designed for developers and product teams who need trustworthy, repeatable user research — not gimmicks.",
    icon: "🔍",
  },
];

const testerSteps = [
  { title: "Create your profile", description: "Tell us about your devices and interests." },
  { title: "Pick a task", description: "Choose testing opportunities that match your profile." },
  { title: "Share feedback & get rewarded", description: "Submit clear feedback and evidence, then get paid." },
];

const companySteps = [
  { title: "Describe your product", description: "Tell RealTest what you want tested and by whom." },
  { title: "Recruit real testers", description: "We match your product with the right testers, fast." },
  { title: "Review structured results", description: "Get organized feedback, bug reports, and evidence back." },
];

export default function Home() {
  return (
    <div className="flex flex-1 flex-col">
      {/* Hero */}
      <section className="border-b border-slate-200 bg-gradient-to-b from-brand-50/60 to-white dark:border-slate-800 dark:from-brand-950/20 dark:to-slate-950">
        <div className="mx-auto flex max-w-6xl flex-col items-center gap-6 px-4 py-20 text-center sm:px-6 sm:py-28">
          <span className="inline-flex items-center rounded-full border border-brand-200 bg-white px-3 py-1 text-xs font-medium text-brand-700 dark:border-brand-800 dark:bg-slate-900 dark:text-brand-300">
            Real people. Real feedback. Real fast.
          </span>
          <h1 className="max-w-3xl text-4xl font-semibold tracking-tight text-slate-900 sm:text-5xl dark:text-slate-50">
            Test your product with real users — not guesswork.
          </h1>
          <p className="max-w-xl text-lg text-slate-600 dark:text-slate-400">
            RealTest connects developers and companies with real testers to try out websites,
            apps, and prototypes, and return honest, structured feedback you can trust.
          </p>
          <div className="flex flex-col gap-3 sm:flex-row">
            <Link href="/register">
              <Button size="lg">I want to test products</Button>
            </Link>
            <Link href="/register">
              <Button size="lg" variant="outline">
                I want users to test my product
              </Button>
            </Link>
          </div>
        </div>
      </section>

      {/* Features */}
      <section className="mx-auto w-full max-w-6xl px-4 py-20 sm:px-6">
        <div className="mx-auto max-w-2xl text-center">
          <h2 className="text-3xl font-semibold tracking-tight text-slate-900 dark:text-slate-50">
            Product research, done right
          </h2>
          <p className="mt-3 text-slate-600 dark:text-slate-400">
            Everything about RealTest is designed to build trust between testers and the teams
            who need their feedback.
          </p>
        </div>
        <div className="mt-12 grid gap-6 sm:grid-cols-2">
          {features.map((feature) => (
            <Card key={feature.title}>
              <CardContent className="flex gap-4">
                <span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-brand-50 text-xl dark:bg-brand-600/10">
                  {feature.icon}
                </span>
                <div>
                  <h3 className="font-semibold text-slate-900 dark:text-slate-50">{feature.title}</h3>
                  <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">{feature.description}</p>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      </section>

      {/* How it works */}
      <section className="border-y border-slate-200 bg-slate-50 dark:border-slate-800 dark:bg-slate-900/40">
        <div className="mx-auto max-w-6xl px-4 py-20 sm:px-6">
          <h2 className="text-center text-3xl font-semibold tracking-tight text-slate-900 dark:text-slate-50">
            How RealTest works
          </h2>

          <div className="mt-14 grid gap-12 lg:grid-cols-2 lg:gap-16">
            <div>
              <h3 className="text-sm font-semibold uppercase tracking-wide text-brand-600">
                For testers
              </h3>
              <ol className="mt-5 flex flex-col gap-6">
                {testerSteps.map((step, index) => (
                  <li key={step.title} className="flex gap-4">
                    <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-brand-600 text-sm font-semibold text-white">
                      {index + 1}
                    </span>
                    <div>
                      <p className="font-medium text-slate-900 dark:text-slate-50">{step.title}</p>
                      <p className="text-sm text-slate-600 dark:text-slate-400">{step.description}</p>
                    </div>
                  </li>
                ))}
              </ol>
            </div>

            <div>
              <h3 className="text-sm font-semibold uppercase tracking-wide text-brand-600">
                For companies
              </h3>
              <ol className="mt-5 flex flex-col gap-6">
                {companySteps.map((step, index) => (
                  <li key={step.title} className="flex gap-4">
                    <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-slate-900 text-sm font-semibold text-white dark:bg-slate-100 dark:text-slate-900">
                      {index + 1}
                    </span>
                    <div>
                      <p className="font-medium text-slate-900 dark:text-slate-50">{step.title}</p>
                      <p className="text-sm text-slate-600 dark:text-slate-400">{step.description}</p>
                    </div>
                  </li>
                ))}
              </ol>
            </div>
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="mx-auto w-full max-w-4xl px-4 py-20 text-center sm:px-6">
        <h2 className="text-3xl font-semibold tracking-tight text-slate-900 dark:text-slate-50">
          Ready to get started?
        </h2>
        <p className="mx-auto mt-3 max-w-xl text-slate-600 dark:text-slate-400">
          Create your free account in minutes — as a tester or as a company.
        </p>
        <div className="mt-8 flex justify-center gap-3">
          <Link href="/register">
            <Button size="lg">Create your account</Button>
          </Link>
          <Link href="/login">
            <Button size="lg" variant="ghost">
              Log in
            </Button>
          </Link>
        </div>
      </section>
    </div>
  );
}
