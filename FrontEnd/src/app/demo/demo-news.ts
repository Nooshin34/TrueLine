import { News } from '../models/news';

const story = (
  id: number,
  title: string,
  summary: string,
  body: string,
  author: string,
  category: News['category'],
  publishedAt: string,
  viewCount: number,
  authorStars: number,
  images: string[],
): News => ({
  id,
  title,
  summary,
  body,
  author,
  category,
  publishedAt,
  isPublished: true,
  isApproved: true,
  viewCount,
  authorStars,
  reporterId: null,
  canEdit: false,
  imageUrl: images[0] ?? null,
  images: images.map((url, index) => ({ id: id * 10 + index + 1, url })),
});

export const demoNews: News[] = [
  story(
    1,
    'The city that retimed its traffic lights',
    'A year of small changes cut the wait at twelve downtown junctions, without adding a single new lane.',
    'For a year, the city traffic desk changed the lights one junction at a time. The goal was modest: shorter waits, not a new skyline of overpasses.\n\nAt the corner of Market and Fifth, the evening queue used to reach the bakery. After the new timing, it usually clears in one cycle. Bus drivers on the river route say the last hour of the shift is quieter.\n\nThe desk published the timings in public, and a group of students checked them with clipboards. The city kept the changes that matched the counts and dropped the ones that only looked good on a chart.',
    'Leila Moradi',
    'Technology',
    '2026-10-05T07:30:00Z',
    1840,
    5,
    ['demo/signals.svg', 'demo/signals-night.svg'],
  ),
  story(
    2,
    'A river town moves its market uphill',
    'Stallholders voted to leave the flood plain after a third wet spring soaked the Saturday books.',
    'The Saturday market spent thirty years on the river stones. When the water rose, people stacked crates and waited. This spring they stopped waiting.\n\nStallholders voted to move the market to the school field, one street up. The town lent tables. Bakers kept their usual places so regulars could still find the almond cakes.\n\nThe old site stays as a walking path. A painted line shows where the water reached in March, so the reason for the move stays visible.',
    'Jonas Keller',
    'World',
    '2026-10-04T09:00:00Z',
    1260,
    4,
    ['demo/market.svg'],
  ),
  story(
    3,
    'Council keeps the night buses for another winter',
    'The last three routes survived a budget vote after hospital staff and bakers filled the public gallery.',
    'The night network was down to three routes and a stack of complaints about empty seats. A vote on Thursday was expected to park the buses until spring.\n\nNurses coming off the late shift, and bakers going on, filled the gallery. They did not argue that every seat was full. They argued that the alternative was a long walk beside a road with no pavement.\n\nThe council kept the routes through winter and asked for a ridership count in March. The first bus still leaves the hospital gate at 12:40.',
    'Amira Hassan',
    'Politics',
    '2026-10-03T18:10:00Z',
    980,
    4,
    ['demo/bus.svg'],
  ),
  story(
    4,
    'Four small presses share one printing floor',
    'A rented press, and a shared calendar, let independent publishers keep paper in a city of rising rents.',
    'Rent on a private print room had pushed four publishers toward digital-only. None of them wanted that for poetry, cookbooks, or the match-day program.\n\nThey leased one floor and one press, then split the week on a shared calendar. Mondays are poetry. Thursdays are the sports pamphlet, which still sells out at the stadium gate.\n\nThe arrangement is dull on purpose: a lock on the paper store, a logbook, and a rule that the machine is cleaned before the next team arrives.',
    'Chris Adeyemi',
    'Business',
    '2026-10-02T11:20:00Z',
    740,
    3,
    ['demo/press.svg'],
  ),
  story(
    5,
    'The marathon that started before dawn',
    'Runners left in the dark so the road could reopen by the time the bakeries unlocked.',
    'The city moved the start to 5:10 so the harbor road could reopen for deliveries. Headlamps made a thin line along the quay, and the first kilometer was mostly the sound of shoes.\n\nVolunteers at the halfway table said the early hour changed the race. People talked less and watched the sky more. The winner finished as the cafes turned their lights on.\n\nNext year the start stays in the dark. The organizers promised a later wave for anyone who wants to run after breakfast.',
    'Marta Silva',
    'Sport',
    '2026-10-01T05:40:00Z',
    2110,
    5,
    ['demo/marathon.svg'],
  ),
  story(
    6,
    'The library lends instruments on Fridays',
    'Violins, clarinets, and a well-used drum kit now sit beside the novels, on a card like any other loan.',
    'Friday afternoons at the central library used to be homework and whispered arguments about whose turn it was on the computers. Now there is also a queue for the violin.\n\nThe instruments came from a school that closed its orchestra. Each one goes out for two weeks, with a card, a shoulder rest, and a note about how to loosen the bow.\n\nStaff keep the drum kit in the building. It can be booked for an hour in the old map room, with the door shut.',
    'Elena Varga',
    'Culture',
    '2026-09-29T15:00:00Z',
    1560,
    5,
    ['demo/library.svg'],
  ),
  story(
    7,
    'Students map the heat on their own street',
    'A class of sixteen-year-olds measured pavements, bus stops, and one playground, then handed the city a map.',
    'The science class was asked to study climate in the abstract. They studied their walk to school instead.\n\nFor two weeks they measured the pavement at the same hour, under the same trees and on the same bare crossings. The playground with no shade was regularly hotter than the street beside it.\n\nThey gave the planning office a one-page map. The office has since put the playground on the list for trees, behind three other streets and ahead of a car park.',
    'Noah Berg',
    'Science',
    '2026-09-28T10:15:00Z',
    890,
    4,
    ['demo/heat.svg'],
  ),
  story(
    8,
    'The evening ferry is back on the closed route',
    'After two summers of a shuttle bus, the boat again ties up at the old steps below the post office.',
    'The evening ferry stopped when the steps cracked. A shuttle bus replaced it, and everyone who used the boat learned the bus was longer, louder, and somehow always full at the second stop.\n\nRepairs finished on Friday. The first return sailing carried a brass band by accident; they had booked the bus and missed it.\n\nThe harbor master says the winter timetable is the old one: last boat at 19:10, except when the wind makes the crossing a bad idea. A board on the steps will say so before anyone buys a ticket.',
    'Hana Suzuki',
    'World',
    '2026-09-26T16:45:00Z',
    1325,
    4,
    ['demo/ferry.svg'],
  ),
];

export function publishedDemoNews(term = ''): News[] {
  const query = term.trim().toLowerCase();
  return demoNews.filter((item) => {
    if (!item.isPublished || !item.isApproved) {
      return false;
    }

    if (!query) {
      return true;
    }

    return [item.title, item.summary ?? '', item.body, item.author, item.category]
      .join('\n')
      .toLowerCase()
      .includes(query);
  });
}

export function demoArticle(id: number): News | null {
  return demoNews.find((item) => item.id === id) ?? null;
}
